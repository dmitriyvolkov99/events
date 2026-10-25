using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security;

public class UserAccessor(IHttpContextAccessor httpContextAccessor) : IUserAccessor
{
    public string GetUsername()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is unavailable.");

        return httpContext.User.FindFirstValue(ClaimTypes.Name)
            ?? throw new InvalidOperationException("Username claim is missing.");
    }
}