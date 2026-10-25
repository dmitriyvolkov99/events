using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Photos;

public class SetMain
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; } = null!;
    }

    public class Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = userAccessor.GetUsername();

            var user = await context.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == username, cancellationToken);

            if (user == null)
            {
                logger.LogWarning(
                    "Failed to set main photo {PhotoId}: user {Username} was not found",
                    request.Id,
                    username);

                return Result<Unit>.Failure("User not found");
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

            if (photo == null)
            {
                logger.LogWarning(
                    "Failed to set main photo {PhotoId}: photo was not found for user {Username}",
                    request.Id,
                    username);

                return Result<Unit>.Failure("Photo not found");
            }

            // уже главное: менять нечего, это не ошибка
            if (photo.IsMain) return Result<Unit>.Success(Unit.Value);

            foreach (var current in user.Photos.Where(x => x.IsMain))
            {
                current.IsMain = false;
            }

            photo.IsMain = true;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Photo {PhotoId} set as main photo by user {Username}", request.Id, username);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}