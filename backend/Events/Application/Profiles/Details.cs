using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles;

public class Details
{
    public class Query : IRequest<Result<Profile>>
    {
        public string Username { get; set; } = null!;
    }

    public class Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor) : IRequestHandler<Query, Result<Profile>>
    {
        public async Task<Result<Profile>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await context.Users.Where(x => x.UserName == request.Username)
                .ProjectTo<Profile>(mapper.ConfigurationProvider, new { currentUsername = userAccessor.GetUsername() })
                .SingleOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

            if (user == null)
            {
                return Result<Profile>.Failure("User not found");

            }

            return Result<Profile>.Success(user);
        }
    }
}