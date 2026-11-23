using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Services;
using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Persistence;

namespace Tests.Support;

public sealed record TestUser(string Username, string Email, string Token);

/// <summary>Настоящее приложение в памяти: SQLite вместо SQL Server и фейк вместо Cloudinary.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string Password = "Pa$$w0rd";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public FakePhotoAccessor Photos { get; } = new();

    public ApiFactory()
    {
        // Program.cs читает эти настройки до того, как фабрика успевает их подменить, поэтому
        // они передаются переменными окружения (во всех фабриках значения одинаковые)
        Environment.SetEnvironmentVariable("TokenKey", new string('k', 64));
        Environment.SetEnvironmentVariable("Cloudinary__CloudName", "test");
        Environment.SetEnvironmentVariable("Cloudinary__ApiKey", "test");
        Environment.SetEnvironmentVariable("Cloudinary__ApiSecret", "test");

        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // убираем SQL Server: DbContext, его параметры и конфигурацию провайдера (EF Core 9+)
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DataContext)
                    || d.ServiceType == typeof(DbContextOptions<DataContext>)
                    || (d.ServiceType.IsGenericType && d.ServiceType.GenericTypeArguments.Contains(typeof(DataContext))))
                .ToList();

            foreach (var descriptor in toRemove) services.Remove(descriptor);

            services.AddDbContext<DataContext>(options => options.UseSqlite(_connection));

            services.RemoveAll<IPhotoAccessor>();
            services.AddSingleton<IPhotoAccessor>(Photos);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<DataContext>().Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }

    /// <summary>Создаёт пользователя напрямую в базе и выпускает ему токен (запросы входа не расходуют лимит).</summary>
    public async Task<TestUser> CreateUserAsync(string? username = null)
    {
        username ??= $"u{Guid.NewGuid():N}"[..13];

        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser { UserName = username, Email = $"{username}@test.com", DisplayName = $"Name {username}" };
        var result = await userManager.CreateAsync(user, Password);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));

        var token = scope.ServiceProvider.GetRequiredService<TokenService>().CreateToken(user);
        return new TestUser(username, user.Email, token);
    }

    public HttpClient ClientFor(TestUser? user = null)
    {
        var client = CreateClient();
        if (user is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
        }

        return client;
    }
}

public static class ApiHelpers
{
    public static object ActivityBody(Guid id, string title = "Picnic", string category = "music") => new
    {
        id,
        title,
        date = DateTime.UtcNow.AddDays(5),
        description = "A nice picnic",
        category,
        city = "Berlin",
        venue = "Park"
    };

    public static async Task<Guid> CreateActivityAsync(this HttpClient client, string title = "Picnic")
    {
        var id = Guid.NewGuid();
        var response = await client.PostAsJsonAsync("/api/activities", ActivityBody(id, title));
        response.EnsureSuccessStatusCode();
        return id;
    }

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();
}