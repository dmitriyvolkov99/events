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

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(
            DataContext context,
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger<Handler> logger)
        {
            _context = context;
            _mapper = mapper;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = _userAccessor.GetUsername();

            var activity = await _context.Activities.FindAsync(
                [request.Activity.Id],
                cancellationToken);

            if (activity == null)
            {
                _logger.LogWarning(
                    "Failed to update activity {ActivityId}: activity was not found. User: {Username}",
                    request.Activity.Id,
                    username);

                return Result<Unit>.Failure("Activity not found");
            }

            _mapper.Map(request.Activity, activity);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                _logger.LogError(
                    "Failed to update activity {ActivityId}. User: {Username}",
                    request.Activity.Id,
                    username);

                return Result<Unit>.Failure(
                    "Failed to update the activity");
            }

            _logger.LogInformation(
                "Activity {ActivityId} updated by user {Username}",
                request.Activity.Id,
                username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}