using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Activities;

public class UpdateAttendance
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var activity = await context.Activities
                .Include(a => a.Attendees)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (activity == null)
            {
                logger.LogWarning(
                    "Failed to update attendance for activity {ActivityId}: activity was not found. User: {Username}",
                    request.Id,
                    username);
                return Result<Unit>.NotFound("Activity not found");
            }

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);

            if (user == null)
            {
                logger.LogWarning(
                    "Failed to update attendance for activity {ActivityId}: user {Username} was not found",
                    request.Id,
                    username);
                return Result<Unit>.Failure("User not found");
            }

            var host = activity.Attendees.FirstOrDefault(x => x.isHost);

            if (host == null)
            {
                logger.LogError("Failed to update attendance for activity {ActivityId}: host was not found", request.Id);
                return Result<Unit>.Failure("Activity host not found");
            }

            var attendance = activity.Attendees.FirstOrDefault(x => x.AppUserId == user.Id);

            if (host.AppUserId == user.Id)
            {
                // организатор: переключает отмену события
                activity.isCancelled = !activity.isCancelled;

                logger.LogInformation(
                    "Activity {ActivityId} cancellation status changed to {IsCancelled} by host {Username}",
                    request.Id,
                    activity.isCancelled,
                    username);
            }
            else if (attendance != null)
            {
                activity.Attendees.Remove(attendance);

                logger.LogInformation("User {Username} stopped attending activity {ActivityId}", username, request.Id);
            }
            else
            {
                if (activity.isCancelled)
                {
                    return Result<Unit>.Failure("You cannot join a cancelled activity");
                }

                activity.Attendees.Add(new ActivityAttendee { AppUser = user, Activity = activity, isHost = false });

                logger.LogInformation("User {Username} started attending activity {ActivityId}", username, request.Id);
            }

            if (await context.SaveChangesAsync(cancellationToken) <= 0)
            {
                logger.LogError(
                    "Failed to update attendance for activity {ActivityId}. User: {Username}",
                    request.Id,
                    username);
                return Result<Unit>.Failure("Problem updating attendance");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}