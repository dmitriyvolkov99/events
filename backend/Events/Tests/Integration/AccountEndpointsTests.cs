using System.Net;
using System.Net.Http.Json;
using Tests.Support;
using Xunit;

namespace Tests.Integration;

// В классе не больше 10 запросов к входу и регистрации: это лимит в минуту на хост
public class AccountEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static object RegisterBody(string username, string password = ApiFactory.Password) => new
    {
        displayName = "Test User",
        username,
        email = $"{username}@test.com",
        password
    };

    [Fact]
    public async Task Registering_returns_a_token_that_the_api_accepts()
    {
        var client = factory.ClientFor();
        var username = $"u{Guid.NewGuid():N}"[..13];

        var response = await client.PostAsJsonAsync("/api/account/register", RegisterBody(username));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.ReadJsonAsync();
        var token = json.GetProperty("token").GetString();
        Assert.False(string.IsNullOrEmpty(token));
        Assert.Equal("Test User", json.GetProperty("displayName").GetString());

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var current = await client.GetAsync("/api/account");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal(username, (await current.ReadJsonAsync()).GetProperty("username").GetString());
    }

    [Fact]
    public async Task A_taken_email_is_rejected()
    {
        var existing = await factory.CreateUserAsync();

        var response = await factory.ClientFor().PostAsJsonAsync(
            "/api/account/register",
            new { displayName = "Other", username = $"u{Guid.NewGuid():N}"[..13], email = existing.Email, password = ApiFactory.Password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Email taken", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task A_too_short_password_is_rejected()
    {
        var response = await factory.ClientFor().PostAsJsonAsync(
            "/api/account/register", RegisterBody($"u{Guid.NewGuid():N}"[..13], "abc"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_password_without_an_uppercase_letter_is_rejected_with_the_identity_reason()
    {
        var response = await factory.ClientFor().PostAsJsonAsync(
            "/api/account/register", RegisterBody($"u{Guid.NewGuid():N}"[..13], "password1"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("PasswordRequiresUpper", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Logging_in_with_the_right_password_returns_a_token()
    {
        var user = await factory.CreateUserAsync();

        var response = await factory.ClientFor().PostAsJsonAsync(
            "/api/account/login", new { email = user.Email, password = ApiFactory.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrEmpty((await response.ReadJsonAsync()).GetProperty("token").GetString()));
    }

    [Fact]
    public async Task A_wrong_password_and_an_unknown_email_look_the_same()
    {
        var user = await factory.CreateUserAsync();
        var client = factory.ClientFor();

        var wrongPassword = await client.PostAsJsonAsync("/api/account/login", new { email = user.Email, password = "Wrong1234" });
        var unknownEmail = await client.PostAsJsonAsync("/api/account/login", new { email = "nobody@test.com", password = "Wrong1234" });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
    }

    [Fact]
    public async Task The_current_user_requires_a_token()
    {
        var response = await factory.ClientFor().GetAsync("/api/account");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class LoginRateLimitTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task More_than_ten_attempts_per_minute_are_rejected_with_429()
    {
        var client = factory.ClientFor();
        var statuses = new List<HttpStatusCode>();

        for (var i = 0; i < 12; i++)
        {
            var response = await client.PostAsJsonAsync(
                "/api/account/login", new { email = "nobody@test.com", password = "Wrong1234" });
            statuses.Add(response.StatusCode);
        }

        Assert.Equal(10, statuses.Count(s => s == HttpStatusCode.Unauthorized));
        Assert.All(statuses.Skip(10), s => Assert.Equal(HttpStatusCode.TooManyRequests, s));
    }
}

public class LoginLockoutTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_even_for_the_right_password()
    {
        var user = await factory.CreateUserAsync();
        var client = factory.ClientFor();

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            var wrong = await client.PostAsJsonAsync("/api/account/login", new { email = user.Email, password = "Wrong1234" });
            Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        }

        // пятая неверная попытка включает блокировку
        var fifth = await client.PostAsJsonAsync("/api/account/login", new { email = user.Email, password = "Wrong1234" });
        Assert.Equal(HttpStatusCode.TooManyRequests, fifth.StatusCode);
        Assert.Contains("Too many failed attempts", await fifth.Content.ReadAsStringAsync());

        var right = await client.PostAsJsonAsync("/api/account/login", new { email = user.Email, password = ApiFactory.Password });
        Assert.Equal(HttpStatusCode.TooManyRequests, right.StatusCode);
    }
}