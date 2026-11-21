using Application.Activities;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Interfaces;
using Infrastructure.Security;
using Infrastructure.Photos;
using API.Configuration;
using Microsoft.Extensions.Options;

namespace API.Extensions;

public static class ApplicationsServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:3000")
                    .WithExposedHeaders("Pagination");
            });
        });

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfiles>();
        });

        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IPhotoAccessor, PhotoAccessor>();

        services.AddOptions<CloudinarySettings>()
            .Bind(config.GetSection("Cloudinary"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<CloudinarySettings>, CloudinarySettingsValidator>();
        services.AddSignalR();

        return services;
    }
}