using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities;

public class Edit
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

    public class Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var activity = await context.Activities.FindAsync([request.Activity.Id], cancellationToken);

            if (activity == null)
            {
                logger.LogWarning(
                    "Failed to update activity {ActivityId}: activity was not found. User: {Username}",
                    request.Activity.Id,
                    username);
                return Result<Unit>.NotFound("Activity not found");
            }

            mapper.Map(request.Activity, activity);

            // SaveChanges возвращает 0, если значения не изменились: это не ошибка
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Activity {ActivityId} updated by user {Username}", request.Activity.Id, username);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}