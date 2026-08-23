using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Comments;

public class Create
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public string Body { get; set; } = null!;
        public Guid ActivityId { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Body).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, Result<CommentDto>>
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
            _userAccessor = userAccessor;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = _userAccessor.GetUsername();

            var activity = await _context.Activities
                .FindAsync(request.ActivityId, cancellationToken);

            if (activity == null)
            {
                _logger.LogWarning(
                    "Failed to create comment: activity {ActivityId} was not found. User: {Username}",
                    request.ActivityId,
                    username);

                return Result<CommentDto>.Failure("Activity not found");
            }

            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (user == null)
            {
                _logger.LogWarning(
                    "Failed to create comment for activity {ActivityId}: user {Username} was not found",
                    request.ActivityId,
                    username);

                return Result<CommentDto>.Failure("User not found");
            }

            var comment = new Comment
            {
                Author = user,
                Activity = activity,
                Body = request.Body
            };

            activity.Comments.Add(comment);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                _logger.LogError(
                    "Failed to create comment for activity {ActivityId}. User: {Username}",
                    request.ActivityId,
                    username);

                return Result<CommentDto>.Failure("Failed to add comment");
            }

            _logger.LogInformation(
                "Comment {CommentId} created on activity {ActivityId} by user {Username}",
                comment.Id,
                request.ActivityId,
                username);

            return Result<CommentDto>.Success(
                _mapper.Map<CommentDto>(comment));
        }
    }
}