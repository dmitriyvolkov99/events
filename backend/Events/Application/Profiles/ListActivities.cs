using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Persistence;
using System;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public class ListActivities
{
    public class Query : IRequest<Result<List<UserActivityDto>>>
    {
        public string Username { get; set; } = null!;
        public string Predicate { get; set; } = null!;
    }

    public class Handler(DataContext context, IMapper mapper) : IRequestHandler<Query, Result<List<UserActivityDto>>>
    {
        public async Task<Result<List<UserActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.ActivityAttendees
                .Where(u => u.AppUser.UserName == request.Username)
                .OrderBy(a => a.Activity.Date)
                .ProjectTo<UserActivityDto>(mapper.ConfigurationProvider)
                .AsQueryable();

            query = request.Predicate switch
            {
                "past" => query.Where(a => a.Date <= DateTime.UtcNow),
                "hosting" => query.Where(a => a.HostUsername == request.Username),
                _ => query.Where(a => a.Date >= DateTime.UtcNow)
            };

            var activities = await query.ToListAsync(cancellationToken);

            return Result<List<UserActivityDto>>.Success(activities);
        }
    }
}