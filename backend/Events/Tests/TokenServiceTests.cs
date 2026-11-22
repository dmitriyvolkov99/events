using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Services;
using Microsoft.IdentityModel.Tokens;
using Tests.Support;
using Xunit;

namespace Tests;

public class TokenServiceTests
{
    private static SymmetricSecurityKey KeyOf(char fill) => new(Encoding.UTF8.GetBytes(new string(fill, 64)));

    private static TokenValidationParameters ParametersFor(SecurityKey key) => new()
    {
        IssuerSigningKey = key,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    private static TokenService CreateService(SecurityKey key, TimeSpan lifetime) =>
        new(new TokenSettings { Key = (SymmetricSecurityKey)key, Lifetime = lifetime });

    [Fact]
    public void The_token_carries_the_user_claims_and_validates_with_the_same_key()
    {
        var key = KeyOf('k');
        var user = TestData.CreateUser("bob");

        var token = CreateService(key, TimeSpan.FromDays(7)).CreateToken(user);

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, ParametersFor(key), out _);
        Assert.Equal("bob", principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal(user.Id, principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("bob@test.com", principal.FindFirstValue(ClaimTypes.Email));
    }

    [Fact]
    public void The_token_expires_after_the_configured_lifetime()
    {
        var key = KeyOf('k');
        var before = DateTime.UtcNow;

        var token = CreateService(key, TimeSpan.FromDays(2)).CreateToken(TestData.CreateUser("bob"));

        var after = DateTime.UtcNow;
        new JwtSecurityTokenHandler().ValidateToken(token, ParametersFor(key), out var validated);
        Assert.InRange(
            validated.ValidTo,
            before.AddDays(2).AddSeconds(-5),
            after.AddDays(2).AddSeconds(5));
    }

    [Fact]
    public void A_token_does_not_validate_with_a_different_key()
    {
        var token = CreateService(KeyOf('k'), TimeSpan.FromDays(7)).CreateToken(TestData.CreateUser("bob"));

        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, ParametersFor(KeyOf('x')), out _));
    }
}