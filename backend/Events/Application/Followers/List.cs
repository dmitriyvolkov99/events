using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Followers;

public class List
{
    public class Query : IRequest<Result<List<Profiles.Profile>>>
    {
        public string Predicate { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<List<Profiles.Profile>>>
    {
        public async Task<Result<List<Profiles.Profile>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = request.Predicate switch
            {
                "followers" => context.UserFollowings
                    .Where(x => x.Target.UserName == request.Username)
                    .Select(x => x.Observer),
                "following" => context.UserFollowings
                    .Where(x => x.Observer.UserName == request.Username)
                    .Select(x => x.Target),
                _ => null
            };

            if (users is null)
            {
                return Result<List<Profiles.Profile>>.Failure("Predicate must be 'followers' or 'following'");
            }

            var profiles = await users
                .ProjectTo<Profiles.Profile>(mapper.ConfigurationProvider, new { currentUsername = userAccessor.GetUsername() })
                .ToListAsync(cancellationToken);

            return Result<List<Profiles.Profile>>.Success(profiles);
        }
    }
}