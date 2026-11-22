using Application.Activities;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests;

public class DeleteActivityTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> DeleteAsync(Guid id, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new Delete.Handler(
            context: context,
            userAccessor: new FakeUserAccessor(username),
            _logger: NullLogger<Delete.Handler>.Instance);
        return await handler.Handle(new Delete.Command { Id = id }, CancellationToken.None);
    }

    [Fact]
    public async Task Deletes_the_activity_with_its_attendees_but_keeps_the_users()
    {
        var activity = TestData.CreateActivity(
            "Event", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"), TestData.CreateUser("tom"));
        await _db.SeedAsync(activity);

        var result = await DeleteAsync(activity.Id, "bob");

        Assert.True(result.IsSuccess);
        await using var verify = _db.CreateContext();
        Assert.Empty(await verify.Activities.ToListAsync());
        Assert.Empty(await verify.ActivityAttendees.ToListAsync());
        Assert.Equal(2, await verify.Users.CountAsync());
    }

    [Fact]
    public async Task Returns_not_found_for_an_unknown_activity()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await DeleteAsync(Guid.NewGuid(), "bob");

        Assert.True(result.IsNotFound);
    }
}