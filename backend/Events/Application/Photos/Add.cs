using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Photos;

public class Add
{
    public class Command : IRequest<Result<Photo>>
    {
        public IFormFile File { get; set; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        private const long MaxFileSize = 10 * 1024 * 1024;
        private static readonly string[] AllowedContentTypes =
            ["image/jpeg", "image/png", "image/webp", "image/gif"];

        public CommandValidator()
        {
            RuleFor(x => x.File)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Choose an image to upload")
                .Must(f => f.Length is > 0 and <= MaxFileSize).WithMessage("The image must be up to 10 MB")
                .Must(f => AllowedContentTypes.Contains(f.ContentType))
                .WithMessage("Only JPEG, PNG, WebP or GIF images are allowed");
        }
    }

    public class Handler(DataContext context, IPhotoAccessor photoAccessor, IUserAccessor userAccessor, ILogger<Handler> logger) : IRequestHandler<Command, Result<Photo>>
    {
        public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var user = await context.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (user == null)
            {
                logger.LogWarning(
                    "Failed to add photo: user {Username} was not found",
                    username);

                return Result<Photo>.Failure("User not found");
            }

            var photoUploadResult = await photoAccessor.AddPhoto(request.File);

            if (photoUploadResult == null)
            {
                logger.LogError(
                    "Failed to upload photo for user {Username}",
                    username);

                return Result<Photo>.Failure("Problem uploading photo");
            }

            var photo = new Photo
            {
                Url = photoUploadResult.Url,
                Id = photoUploadResult.PublicId
            };

            if (!user.Photos.Any(x => x.IsMain))
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                logger.LogError(
                    "Photo upload succeeded but database save failed for user {Username}. PhotoId: {PhotoId}",
                    username,
                    photo.Id);

                return Result<Photo>.Failure(
                    "Problem adding photo");
            }

            logger.LogInformation(
                "Photo {PhotoId} added by user {Username}. IsMain: {IsMain}",
                photo.Id,
                username,
                photo.IsMain);

            return Result<Photo>.Success(photo);
        }
    }
}