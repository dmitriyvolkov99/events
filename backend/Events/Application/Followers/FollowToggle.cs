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

    public class Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var observer = await context.Users
                .FirstOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (observer == null)
            {
                logger.LogWarning(
                    "Failed to update following: observer {Username} was not found",
                    username);

                return Result<Unit>.Failure("Observer not found");
            }

            var target = await context.Users
                .FirstOrDefaultAsync(
                    x => x.UserName == request.TargetUsername,
                    cancellationToken);

            if (target == null)
            {
                logger.LogWarning(
                    "Failed to update following: target user {TargetUsername} was not found. Observer: {Username}",
                    request.TargetUsername,
                    username);

                return Result<Unit>.Failure("Target user not found");
            }

            if (observer.Id == target.Id)
            {
                return Result<Unit>.Failure("You cannot follow yourself");
            }

            var following = await context.UserFollowings.FindAsync([observer.Id, target.Id], cancellationToken);

            var startedFollowing = following == null;

            if (following == null)
            {
                following = new UserFollowing
                {
                    Observer = observer,
                    Target = target
                };

                context.UserFollowings.Add(following);
            }
            else
            {
                context.UserFollowings.Remove(following);
            }

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                logger.LogError(
                    "Failed to update following from {Username} to {TargetUsername}",
                    username,
                    request.TargetUsername);

                return Result<Unit>.Failure(
                    "Failed to update following");
            }

            if (startedFollowing)
            {
                logger.LogInformation(
                    "User {Username} started following user {TargetUsername}",
                    username,
                    request.TargetUsername);
            }
            else
            {
                logger.LogInformation(
                    "User {Username} stopped following user {TargetUsername}",
                    username,
                    request.TargetUsername);
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}