using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public sealed class TokenSettings
{
    public required SymmetricSecurityKey Key { get; init; }
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromDays(7);
}