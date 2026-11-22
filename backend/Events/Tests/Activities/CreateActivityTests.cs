using Application.Activities;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests;

public class CreateActivityTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> CreateAsync(Activity activity, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new Create.Handler(
            context: context,
            userAccessor: new FakeUserAccessor(username),
            logger: NullLogger<Create.Handler>.Instance);
        return await handler.Handle(new Create.Command { Activity = activity }, CancellationToken.None);
    }

    [Fact]
    public async Task Creates_the_activity_and_makes_the_current_user_its_host()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await CreateAsync(TestData.NewActivity("Picnic"), "bob");

        Assert.True(result.IsSuccess);
        await using var verify = _db.CreateContext();
        var saved = await verify.Activities.Include(a => a.Attendees).ThenInclude(a => a.AppUser).SingleAsync();
        Assert.Equal("Picnic", saved.Title);
        var attendee = Assert.Single(saved.Attendees);
        Assert.True(attendee.IsHost);
        Assert.Equal("bob", attendee.AppUser.UserName);
    }

    [Fact]
    public async Task Fails_when_the_current_user_does_not_exist()
    {
        var result = await CreateAsync(TestData.NewActivity(), "ghost");

        Assert.False(result.IsSuccess);
        await using var verify = _db.CreateContext();
        Assert.Empty(await verify.Activities.ToListAsync());
    }

    [Fact]
    public async Task Fails_when_the_id_is_already_used()
    {
        var existing = TestData.CreateActivity("Existing", DateTime.UtcNow.AddDays(1), TestData.CreateUser("bob"));
        await _db.SeedAsync(existing);
        var duplicate = TestData.NewActivity("Duplicate");
        duplicate.Id = existing.Id;

        var result = await CreateAsync(duplicate, "bob");

        Assert.False(result.IsSuccess);
        await using var verify = _db.CreateContext();
        Assert.Equal("Existing", (await verify.Activities.SingleAsync()).Title);
    }
}