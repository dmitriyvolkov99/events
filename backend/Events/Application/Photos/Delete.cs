using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Photos;

public class Delete
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; } = null!;
    }

    public class Handler(DataContext context, IPhotoAccessor photoAccessor, IUserAccessor userAccessor, ILogger<Handler> logger) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
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
                    "Failed to delete photo {PhotoId}: user {Username} was not found",
                    request.Id,
                    username);

                return Result<Unit>.Failure("User not found");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

            if (photo == null)
            {
                logger.LogWarning(
                   "Failed to delete photo {PhotoId}: photo was not found for user {Username}",
                   request.Id,
                   username);

                return Result<Unit>.Failure("Photo not found");
            }

            if (photo.IsMain)
            {
                logger.LogWarning(
                    "User {Username} attempted to delete main photo {PhotoId}",
                    username,
                    request.Id);

                return Result<Unit>.Failure("You cannot delete your main photo");
            }

            context.Photos.Remove(photo);
            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                logger.LogError("Failed to delete photo {PhotoId} from the database. User: {Username}", photo.Id, username);
                return Result<Unit>.Failure("Problem deleting photo");
            }

            var result = await photoAccessor.DeletePhoto(photo.Id);
            if (result == null)
            {
                logger.LogWarning("Photo {PhotoId} removed from the database but not from Cloudinary", photo.Id);
            }

            logger.LogInformation("Photo {PhotoId} deleted by user {Username}", photo.Id, username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}