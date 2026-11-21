using System.Text;
using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<AppUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 8;
            opt.User.RequireUniqueEmail = true;
            opt.Lockout.MaxFailedAccessAttempts = 5;
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddEntityFrameworkStores<DataContext>()
        .AddSignInManager<SignInManager<AppUser>>();

        var tokenSettings = CreateTokenSettings(config);
        services.AddSingleton(tokenSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = tokenSettings.Key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("isActivityHost", policy => policy.Requirements.Add(new HostRequirement()));

        services.AddTransient<IAuthorizationHandler, HostRequirementHandler>();
        services.AddSingleton<TokenService>();

        return services;
    }

    private static TokenSettings CreateTokenSettings(IConfiguration config)
    {
        var tokenKey = config["TokenKey"];
        if (string.IsNullOrWhiteSpace(tokenKey) || Encoding.UTF8.GetByteCount(tokenKey) < 64)
        {
            throw new InvalidOperationException(
                "TokenKey must be configured and be at least 64 bytes long (HMAC-SHA512).");
        }

        var lifetimeDays = config.GetValue("TokenLifetimeDays", 7);
        if (lifetimeDays is < 1 or > 90)
        {
            throw new InvalidOperationException("TokenLifetimeDays must be between 1 and 90.");
        }

        return new TokenSettings
        {
            Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            Lifetime = TimeSpan.FromDays(lifetimeDays)
        };
    }
}