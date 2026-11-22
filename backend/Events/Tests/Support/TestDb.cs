using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.Support;

/// <summary>База SQLite в памяти: у каждого теста чистая база с настоящими ограничениями и каскадами.</summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<DataContext> _options;

    public TestDb()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<DataContext>().UseSqlite(_connection).Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>Новый контекст на той же базе: проверяется то, что записано в БД, а не кэш EF.</summary>
    public DataContext CreateContext() => new(_options);

    public async Task SeedAsync(params object[] entities)
    {
        await using var context = CreateContext();
        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    public void Dispose() => _connection.Dispose();
}