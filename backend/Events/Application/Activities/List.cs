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

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<PagedList<ActivityDto>>>
        {
            public ActivityParams Params { get; set; } = null!;
        }

        public class Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            : IRequestHandler<Query, Result<PagedList<ActivityDto>>>
        {
            public async Task<Result<PagedList<ActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var username = userAccessor.GetUsername();
                var filter = request.Params;

                var query = context.Activities
                    .Where(a => a.Date >= filter.StartDate)
                    .OrderBy(a => a.Date)
                    .ProjectTo<ActivityDto>(mapper.ConfigurationProvider, new { currentUsername = username });

                if (filter.IsGoing && !filter.IsHost)
                {
                    query = query.Where(x => x.Attendees.Any(a => a.Username == username));
                }

                if (filter.IsHost && !filter.IsGoing)
                {
                    query = query.Where(x => x.HostUsername == username);
                }

                var page = await PagedList<ActivityDto>.CreateAsync(
                    query, filter.PageNumber, filter.PageSize, cancellationToken);

                return Result<PagedList<ActivityDto>>.Success(page);
            }
        }
    }
}