using Application.Core;
using Application.Followers;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests;

public class FollowerTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> ToggleAsync(string observer, string target)
    {
        await using var context = _db.CreateContext();
        var handler = new FollowToggle.Handler(
            context: context,
            userAccessor: new FakeUserAccessor(observer),
            logger: NullLogger<FollowToggle.Handler>.Instance);
        return await handler.Handle(new FollowToggle.Command { TargetUsername = target }, CancellationToken.None);
    }

    private async Task<int> CountFollowingsAsync()
    {
        await using var context = _db.CreateContext();
        return await context.UserFollowings.CountAsync();
    }

    private async Task<string[]> ListUsernamesAsync(string predicate, string username, string currentUser)
    {
        await using var context = _db.CreateContext();
        var handler = new List.Handler(
            context: context,
            mapper: TestMapper.Instance,
            userAccessor: new FakeUserAccessor(currentUser));
        var result = await handler.Handle(
            new List.Query { Predicate = predicate, Username = username }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value!.Select(p => p.Username).OrderBy(n => n).ToArray();
    }

    private Task SeedGraphAsync()
    {
        // tom и anna подписаны на bob, bob подписан на anna
        var bob = TestData.CreateUser("bob");
        var tom = TestData.CreateUser("tom");
        var anna = TestData.CreateUser("anna");
        return _db.SeedAsync(
            new UserFollowing { Observer = tom, Target = bob },
            new UserFollowing { Observer = anna, Target = bob },
            new UserFollowing { Observer = bob, Target = anna });
    }

    [Fact]
    public async Task Following_creates_the_relation()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"), TestData.CreateUser("tom"));

        var result = await ToggleAsync("tom", "bob");

        Assert.True(result.IsSuccess);
        Assert.Equal(1, await CountFollowingsAsync());
    }

    [Fact]
    public async Task Following_again_removes_the_relation()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"), TestData.CreateUser("tom"));
        await ToggleAsync("tom", "bob");

        var result = await ToggleAsync("tom", "bob");

        Assert.True(result.IsSuccess);
        Assert.Equal(0, await CountFollowingsAsync());
    }

    [Fact]
    public async Task A_user_cannot_follow_themselves()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await ToggleAsync("bob", "bob");

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await CountFollowingsAsync());
    }

    [Fact]
    public async Task Following_an_unknown_user_fails()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await ToggleAsync("bob", "ghost");

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await CountFollowingsAsync());
    }

    [Fact]
    public async Task An_unknown_observer_fails()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await ToggleAsync("ghost", "bob");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Lists_the_followers_of_a_user()
    {
        await SeedGraphAsync();

        Assert.Equal(new[] { "anna", "tom" }, await ListUsernamesAsync("followers", "bob", "bob"));
    }

    [Fact]
    public async Task Lists_the_users_someone_follows()
    {
        await SeedGraphAsync();

        Assert.Equal(new[] { "anna" }, await ListUsernamesAsync("following", "bob", "bob"));
    }

    [Fact]
    public async Task An_unknown_predicate_is_rejected()
    {
        await using var context = _db.CreateContext();
        var handler = new List.Handler(
            context: context,
            mapper: TestMapper.Instance,
            userAccessor: new FakeUserAccessor("bob"));

        var result = await handler.Handle(
            new List.Query { Predicate = "enemies", Username = "bob" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}