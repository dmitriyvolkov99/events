using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

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

    public class Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("Failed to create activity: user {Username} was not found", username);
                return Result<Unit>.Failure("User not found");
            }

            if (await context.Activities.AnyAsync(x => x.Id == request.Activity.Id, cancellationToken))
            {
                logger.LogWarning(
                    "Failed to create activity: id {ActivityId} is already used. User: {Username}",
                    request.Activity.Id,
                    username);
                return Result<Unit>.Failure("An activity with this id already exists");
            }

            request.Activity.Attendees.Add(new ActivityAttendee
            {
                AppUser = user,
                Activity = request.Activity,
                isHost = true
            });

            context.Activities.Add(request.Activity);

            if (await context.SaveChangesAsync(cancellationToken) <= 0)
            {
                logger.LogError("Failed to create activity for user {Username}", username);
                return Result<Unit>.Failure("Failed to create activity");
            }

            logger.LogInformation("Activity {ActivityId} created by user {Username}", request.Activity.Id, username);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}