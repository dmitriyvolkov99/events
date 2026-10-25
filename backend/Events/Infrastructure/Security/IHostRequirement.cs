using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Security;

public class HostRequirement : IAuthorizationRequirement;

public class HostRequirementHandler(DataContext dbContext, IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<HostRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HostRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var httpContext = httpContextAccessor.HttpContext;

        if (userId is null || httpContext is null) return;

        if (!Guid.TryParse(httpContext.GetRouteValue("id")?.ToString(), out var activityId)) return;

        var isHost = await dbContext.ActivityAttendees
            .AsNoTracking()
            .AnyAsync(
                x => x.AppUserId == userId && x.ActivityId == activityId && x.isHost,
                httpContext.RequestAborted);

        if (isHost) context.Succeed(requirement);
    }
}