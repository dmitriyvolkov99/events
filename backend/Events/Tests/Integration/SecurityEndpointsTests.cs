using System.Net;
using Tests.Support;
using Xunit;

namespace Tests.Integration;

public class SecurityEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Activities_require_a_token()
    {
        var response = await factory.ClientFor().GetAsync("/api/activities");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_valid_token_is_accepted()
    {
        var user = await factory.CreateUserAsync();

        var response = await factory.ClientFor(user).GetAsync("/api/activities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task A_garbage_token_is_rejected()
    {
        var client = factory.ClientFor(new TestUser("x", "x@test.com", "not-a-token"));

        var response = await client.GetAsync("/api/activities");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_reports_a_working_database()
    {
        var response = await factory.ClientFor().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Chat_negotiation_requires_a_token()
    {
        var response = await factory.ClientFor().PostAsync("/chat/negotiate?negotiateVersion=1", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Chat_negotiation_accepts_a_token_from_the_query_string()
    {
        var user = await factory.CreateUserAsync();

        var response = await factory.ClientFor()
            .PostAsync($"/chat/negotiate?negotiateVersion=1&access_token={user.Token}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task The_api_description_is_not_exposed_outside_development()
    {
        var response = await factory.ClientFor().GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}