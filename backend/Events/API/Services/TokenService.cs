using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(TokenSettings settings)
{
    public string CreateToken(AppUser user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!)
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(settings.Lifetime),
            SigningCredentials = new SigningCredentials(settings.Key, SecurityAlgorithms.HmacSha512Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}