using Application.Core;
using Application.Interfaces;
using Domain;
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

    public class Handler : IRequestHandler<Command, Result<Photo>>
    {
        private readonly DataContext _context;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(
            DataContext context,
            IPhotoAccessor photoAccessor,
            IUserAccessor userAccessor,
            ILogger<Handler> logger)
        {
            _userAccessor = userAccessor;
            _photoAccessor = photoAccessor;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = _userAccessor.GetUsername();

            var user = await _context.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (user == null)
            {
                _logger.LogWarning(
                    "Failed to add photo: user {Username} was not found",
                    username);

                return Result<Photo>.Failure("User not found");
            }

            var photoUploadResult = await _photoAccessor.AddPhoto(request.File);

            if (photoUploadResult == null)
            {
                _logger.LogError(
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

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                _logger.LogError(
                    "Photo upload succeeded but database save failed for user {Username}. PhotoId: {PhotoId}",
                    username,
                    photo.Id);

                return Result<Photo>.Failure(
                    "Problem adding photo");
            }

            _logger.LogInformation(
                "Photo {PhotoId} added by user {Username}. IsMain: {IsMain}",
                photo.Id,
                username,
                photo.IsMain);

            return Result<Photo>.Success(photo);
        }
    }
}