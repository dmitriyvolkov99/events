using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Followers;

public class FollowToggle
{
    public class Command : IRequest<Result<Unit>>
    {
        public string TargetUsername { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(
            DataContext context,
            IUserAccessor userAccessor,
            ILogger<Handler> logger)
        {
            _userAccessor = userAccessor;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = _userAccessor.GetUsername();

            var observer = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (observer == null)
            {
                _logger.LogWarning(
                    "Failed to update following: observer {Username} was not found",
                    username);

                return Result<Unit>.Failure("Observer not found");
            }

            var target = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.UserName == request.TargetUsername,
                    cancellationToken);

            if (target == null)
            {
                _logger.LogWarning(
                    "Failed to update following: target user {TargetUsername} was not found. Observer: {Username}",
                    request.TargetUsername,
                    username);

                return Result<Unit>.Failure("Target user not found");
            }

            var following = await _context.UserFollowings.FindAsync(observer.Id, target.Id, cancellationToken);

            var isFollowing = following == null;

            if (following == null)
            {
                following = new UserFollowing
                {
                    Observer = observer,
                    Target = target
                };

                _context.UserFollowings.Add(following);
            }
            else
            {
                _context.UserFollowings.Remove(following);
            }

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                _logger.LogError(
                    "Failed to update following from {Username} to {TargetUsername}",
                    username,
                    request.TargetUsername);

                return Result<Unit>.Failure(
                    "Failed to update following");
            }

            if (isFollowing)
            {
                _logger.LogInformation(
                    "User {Username} started following user {TargetUsername}",
                    username,
                    request.TargetUsername);
            }
            else
            {
                _logger.LogInformation(
                    "User {Username} stopped following user {TargetUsername}",
                    username,
                    request.TargetUsername);
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}