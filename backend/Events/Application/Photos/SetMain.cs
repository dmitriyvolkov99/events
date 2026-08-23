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

public class SetMain
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(
            DataContext context,
            IUserAccessor userAccessor,
            ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
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
                    "Failed to set main photo {PhotoId}: user {Username} was not found",
                    request.Id,
                    username);

                return Result<Unit>.Failure("User not found");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

            if (photo == null)
            {
                _logger.LogWarning(
                    "Failed to set main photo {PhotoId}: photo was not found for user {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure("Photo not found");
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            currentMain?.IsMain = false;

            photo.IsMain = true;

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                _logger.LogError(
                    "Failed to set photo {PhotoId} as main for user {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure(
                    "Problem setting main photo");
            }

            _logger.LogInformation(
                "Photo {PhotoId} set as main photo by user {Username}",
                request.Id,
                username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}