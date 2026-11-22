using Application.Core;
using Application.Photos;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests.Photoes;

public class SetMainPhotoTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> SetMainAsync(string photoId, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new SetMain.Handler(
            context: context,
            userAccessor: new FakeUserAccessor(username),
            logger: NullLogger<SetMain.Handler>.Instance);
        return await handler.Handle(new SetMain.Command { Id = photoId }, CancellationToken.None);
    }

    private async Task<Dictionary<string, bool>> MainFlagsAsync()
    {
        await using var context = _db.CreateContext();
        return await context.Photos.ToDictionaryAsync(p => p.Id, p => p.IsMain);
    }

    [Fact]
    public async Task Makes_the_photo_main_and_unsets_the_previous_one()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await SetMainAsync("p2", "bob");

        Assert.True(result.IsSuccess);
        var flags = await MainFlagsAsync();
        Assert.True(flags["p2"]);
        Assert.False(flags["p1"]);
        Assert.True(flags["t1"]);
    }

    [Fact]
    public async Task Setting_the_current_main_photo_again_is_a_success()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await SetMainAsync("p1", "bob");

        Assert.True(result.IsSuccess);
        var flags = await MainFlagsAsync();
        Assert.True(flags["p1"]);
        Assert.False(flags["p2"]);
    }

    [Fact]
    public async Task A_user_cannot_change_the_main_photo_to_someone_elses_photo()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await SetMainAsync("p2", "tom");

        Assert.False(result.IsSuccess);
        var flags = await MainFlagsAsync();
        Assert.True(flags["p1"]);
        Assert.False(flags["p2"]);
        Assert.True(flags["t1"]);
    }
}

public class DeletePhotoTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly FakePhotoAccessor _cloud = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> DeleteAsync(string photoId, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new Delete.Handler(
            context: context,
            photoAccessor: _cloud,
            userAccessor: new FakeUserAccessor(username),
            logger: NullLogger<Delete.Handler>.Instance);
        return await handler.Handle(new Delete.Command { Id = photoId }, CancellationToken.None);
    }

    private async Task<string[]> PhotoIdsAsync()
    {
        await using var context = _db.CreateContext();
        return (await context.Photos.Select(p => p.Id).ToListAsync()).OrderBy(id => id).ToArray();
    }

    [Fact]
    public async Task Deletes_a_photo_from_the_database_and_from_the_cloud()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await DeleteAsync("p2", "bob");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "p1", "t1" }, await PhotoIdsAsync());
        Assert.Equal(new[] { "p2" }, _cloud.Deleted.ToArray());
    }

    [Fact]
    public async Task The_main_photo_cannot_be_deleted()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await DeleteAsync("p1", "bob");

        Assert.False(result.IsSuccess);
        Assert.Equal(new[] { "p1", "p2", "t1" }, await PhotoIdsAsync());
        Assert.Empty(_cloud.Deleted);
    }

    [Fact]
    public async Task A_user_cannot_delete_someone_elses_photo()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await DeleteAsync("p2", "tom");

        Assert.False(result.IsSuccess);
        Assert.Equal(new[] { "p1", "p2", "t1" }, await PhotoIdsAsync());
        Assert.Empty(_cloud.Deleted);
    }

    [Fact]
    public async Task An_unknown_photo_is_rejected()
    {
        await TestData.SeedPhotoOwnersAsync(_db);

        var result = await DeleteAsync("nope", "bob");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task The_record_is_removed_even_when_the_cloud_cleanup_fails()
    {
        await TestData.SeedPhotoOwnersAsync(_db);
        _cloud.DeleteSucceeds = false;

        var result = await DeleteAsync("p2", "bob");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "p1", "t1" }, await PhotoIdsAsync());
    }
}