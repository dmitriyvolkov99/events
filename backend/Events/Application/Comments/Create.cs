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

    public class Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger) : IRequestHandler<Command, Result<CommentDto>>
    {
        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var activity = await context.Activities
                .FindAsync([request.ActivityId], cancellationToken);

            if (activity == null)
            {
                logger.LogWarning(
                    "Failed to create comment: activity {ActivityId} was not found. User: {Username}",
                    request.ActivityId,
                    username);

                return Result<CommentDto>.Failure("Activity not found");
            }

            var user = await context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(
                    x => x.UserName == username,
                    cancellationToken);

            if (user == null)
            {
                logger.LogWarning(
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

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
            {
                logger.LogError(
                    "Failed to create comment for activity {ActivityId}. User: {Username}",
                    request.ActivityId,
                    username);

                return Result<CommentDto>.Failure("Failed to add comment");
            }

            logger.LogInformation(
                "Comment {CommentId} created on activity {ActivityId} by user {Username}",
                comment.Id,
                request.ActivityId,
                username);

            return Result<CommentDto>.Success(
                mapper.Map<CommentDto>(comment));
        }
    }
}