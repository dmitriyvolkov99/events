using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities;

public class Delete
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(DataContext context, ILogger<Handler> _logger, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var activity = await context.Activities.FindAsync(
                [request.Id],
                cancellationToken);

            if (activity == null)
            {
                _logger.LogWarning(
                    "Failed to delete activity {ActivityId}: activity was not found. User: {Username}",
                    request.Id,
                    username);

                return Result<Unit>.NotFound("Activity not found");
            }

            context.Remove(activity);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                _logger.LogError(
                    "Failed to delete activity {ActivityId}. User: {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure("Failed to delete the activity");
            }

            _logger.LogInformation(
                "Activity {ActivityId} deleted by user {Username}",
                request.Id,
                username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}