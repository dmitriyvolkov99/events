using System.Net;
using Tests.Support;
using Xunit;

namespace Tests.Integration;

public class ProfileEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static async Task<(bool Following, int Followers)> ProfileOfAsync(HttpClient client, string username)
    {
        var json = await (await client.GetAsync($"/api/profiles/{username}")).ReadJsonAsync();
        return (json.GetProperty("following").GetBoolean(), json.GetProperty("followersCount").GetInt32());
    }

    [Fact]
    public async Task A_user_cannot_follow_themselves()
    {
        var user = await factory.CreateUserAsync();

        var response = await factory.ClientFor(user).PostAsync($"/api/follow/{user.Username}", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Following_and_unfollowing_changes_the_counters()
    {
        var target = await factory.CreateUserAsync();
        var follower = factory.ClientFor(await factory.CreateUserAsync());

        Assert.Equal(HttpStatusCode.OK, (await follower.PostAsync($"/api/follow/{target.Username}", null)).StatusCode);
        Assert.Equal((true, 1), await ProfileOfAsync(follower, target.Username));

        Assert.Equal(HttpStatusCode.OK, (await follower.PostAsync($"/api/follow/{target.Username}", null)).StatusCode);
        Assert.Equal((false, 0), await ProfileOfAsync(follower, target.Username));
    }

    [Fact]
    public async Task Following_an_unknown_user_fails()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.PostAsync("/api/follow/nobody-here", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task An_unknown_predicate_is_rejected()
    {
        var user = await factory.CreateUserAsync();

        var response = await factory.ClientFor(user).GetAsync($"/api/follow/{user.Username}?predicate=enemies");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task An_unknown_profile_is_a_404()
    {
        var client = factory.ClientFor(await factory.CreateUserAsync());

        var response = await client.GetAsync("/api/profiles/nobody-here");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}