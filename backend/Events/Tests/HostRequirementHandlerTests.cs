using System.Security.Claims;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Tests.Support;
using Xunit;

namespace Tests;

public class HostRequirementHandlerTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<bool> AuthorizeAsync(string? userId, string? routeId)
    {
        var httpContext = new DefaultHttpContext();
        if (routeId is not null) httpContext.Request.RouteValues["id"] = routeId;

        await using var dbContext = _db.CreateContext();
        var handler = new HostRequirementHandler(
            dbContext: dbContext,
            httpContextAccessor: new HttpContextAccessor { HttpContext = httpContext });

        Claim[] claims = userId is null ? [] : [new Claim(ClaimTypes.NameIdentifier, userId)];
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var context = new AuthorizationHandlerContext([new HostRequirement()], user, null);

        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    private async Task<(Guid ActivityId, AppUser Host, AppUser Attendee, AppUser Outsider)> SeedAsync()
    {
        var host = TestData.CreateUser("bob");
        var attendee = TestData.CreateUser("tom");
        var outsider = TestData.CreateUser("anna");
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), host, attendee);
        await _db.SeedAsync(activity, outsider);
        return (activity.Id, host, attendee, outsider);
    }

    [Fact]
    public async Task The_host_is_authorized()
    {
        var (activityId, host, _, _) = await SeedAsync();

        Assert.True(await AuthorizeAsync(host.Id, activityId.ToString()));
    }

    [Fact]
    public async Task A_regular_attendee_is_not_authorized()
    {
        var (activityId, _, attendee, _) = await SeedAsync();

        Assert.False(await AuthorizeAsync(attendee.Id, activityId.ToString()));
    }

    [Fact]
    public async Task A_user_who_is_not_attending_is_not_authorized()
    {
        var (activityId, _, _, outsider) = await SeedAsync();

        Assert.False(await AuthorizeAsync(outsider.Id, activityId.ToString()));
    }

    [Fact]
    public async Task An_unknown_activity_is_not_authorized()
    {
        var (_, host, _, _) = await SeedAsync();

        Assert.False(await AuthorizeAsync(host.Id, Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task An_anonymous_user_is_not_authorized()
    {
        var (activityId, _, _, _) = await SeedAsync();

        Assert.False(await AuthorizeAsync(null, activityId.ToString()));
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData(null)]
    public async Task A_missing_or_malformed_id_is_denied_without_an_exception(string? routeId)
    {
        var (_, host, _, _) = await SeedAsync();

        Assert.False(await AuthorizeAsync(host.Id, routeId));
    }
}