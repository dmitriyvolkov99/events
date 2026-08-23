using Application.Core;
using Application.Interfaces;
using Domain;
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

            var activity = await _context.Activities.FindAsync(
                [request.Id],
                cancellationToken);

            if (activity == null)
            {
                _logger.LogWarning(
                    "Failed to delete activity {ActivityId}: activity was not found. User: {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure("Activity not found");
            }

            _context.Remove(activity);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

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