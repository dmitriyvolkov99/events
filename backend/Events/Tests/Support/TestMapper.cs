using Application.Core;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Support;

public static class TestMapper
{
    public static IMapper Instance { get; } = Create();

    private static IMapper Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }
}