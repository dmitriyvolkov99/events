using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities;

public class Create
{
    public class Command : IRequest<Result<Unit>>
    {
        public Activity Activity { get; set; } = null!;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
        }
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

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (user == null)
            {
                _logger.LogWarning(
                    "Failed to create activity: user {Username} was not found",
                    username);

                return Result<Unit>.Failure("User not found");
            }

            var attendee = new ActivityAttendee
            {
                AppUser = user,
                Activity = request.Activity,
                isHost = true
            };

            request.Activity.Attendees.Add(attendee);

            _context.Activities.Add(request.Activity);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                _logger.LogError(
                    "Failed to create activity for user {Username}",
                    username);

                return Result<Unit>.Failure("Failed to create activity");
            }

            _logger.LogInformation(
                "Activity {ActivityId} created by user {Username}",
                request.Activity.Id,
                username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}