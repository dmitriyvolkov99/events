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

    public class Handler : IRequestHandler<Command, Result<Unit>>
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

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
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
                    "Failed to delete photo {PhotoId}: user {Username} was not found",
                    request.Id,
                    username);

                return Result<Unit>.Failure("User not found");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

            if (photo == null)
            {
                _logger.LogWarning(
                   "Failed to delete photo {PhotoId}: photo was not found for user {Username}",
                   request.Id,
                   username);

                return Result<Unit>.Failure("Photo not found");
            }

            if (photo.IsMain)
            {
                _logger.LogWarning(
                    "User {Username} attempted to delete main photo {PhotoId}",
                    username,
                    request.Id);

                return Result<Unit>.Failure("You cannot delete your main photo");
            }

            var result = await _photoAccessor.DeletePhoto(photo.Id);

            if (result == null)
            {
                _logger.LogError(
                    "Failed to delete photo {PhotoId} from Cloudinary. User: {Username}",
                    photo.Id,
                    username);

                return Result<Unit>.Failure("Problem deleting photo from Cloudinary");
            }

            _context.Photos.Remove(photo);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                _logger.LogError(
                    "Photo {PhotoId} was deleted from Cloudinary but failed to delete from database. User: {Username}",
                    photo.Id,
                    username);

                return Result<Unit>.Failure(
                    "Problem deleting photo from API");
            }

            _logger.LogInformation(
                "Photo {PhotoId} deleted by user {Username}",
                photo.Id,
                username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}