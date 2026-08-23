using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities;

public class UpdateAttendance
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
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
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = _userAccessor.GetUsername();

            var activity = await _context.Activities
                .Include(a => a.Attendees)
                .ThenInclude(u => u.AppUser)
                .SingleOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

            if (activity == null)
            {
                _logger.LogWarning(
                    "Failed to update attendance for activity {ActivityId}: activity was not found. User: {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure("Activity not found");
            }

            var user = await _context.Users
               .FirstOrDefaultAsync(
                   x => x.UserName == username,
                   cancellationToken);

            if (user == null)
            {
                _logger.LogWarning(
                    "Failed to update attendance for activity {ActivityId}: user {Username} was not found",
                    request.Id,
                    username);

                return Result<Unit>.Failure("User not found");
            }

            var host = activity.Attendees
                .FirstOrDefault(x => x.isHost);

            if (host == null)
            {
                _logger.LogError(
                    "Failed to update attendance for activity {ActivityId}: activity host was not found",
                    request.Id);

                return Result<Unit>.Failure("Activity host not found");
            }

            var attendance = activity.Attendees
                .FirstOrDefault(x => x.AppUserId == user.Id);

            if (attendance != null && host.AppUserId == user.Id)
            {
                activity.isCancelled = !activity.isCancelled;

                _logger.LogInformation(
                    "Activity {ActivityId} cancellation status changed to {IsCancelled} by host {Username}",
                    request.Id,
                    activity.isCancelled,
                    username);
            }

            if (attendance != null && host.AppUserId != user.Id)
            {
                activity.Attendees.Remove(attendance);

                _logger.LogInformation(
                    "User {Username} stopped attending activity {ActivityId}",
                    username,
                    request.Id);
            }

            if (attendance == null)
            {
                attendance = new ActivityAttendee
                {
                    AppUser = user,
                    Activity = activity,
                    isHost = false
                };

                activity.Attendees.Add(attendance);

                _logger.LogInformation(
                    "User {Username} started attending activity {ActivityId}",
                    username,
                    request.Id);
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                _logger.LogError(
                    "Failed to update attendance for activity {ActivityId}. User: {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure(
                    "Problem updating attendance");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}