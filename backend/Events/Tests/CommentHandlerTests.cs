using Application.Comments;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests;

public class CommentHandlerTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<CommentDto>> CreateAsync(Guid activityId, string body, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new Create.Handler(
            context: context,
            mapper: TestMapper.Instance,
            userAccessor: new FakeUserAccessor(username),
            logger: NullLogger<Create.Handler>.Instance);
        return await handler.Handle(new Create.Command { ActivityId = activityId, Body = body }, CancellationToken.None);
    }

    private async Task<CommentDto[]> ListAsync(Guid activityId)
    {
        await using var context = _db.CreateContext();
        var handler = new Application.Comments.List.Handler(context: context, mapper: TestMapper.Instance);
        var result = await handler.Handle(new Application.Comments.List.Query { ActivityId = activityId }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value!.ToArray();
    }

    private static Comment CommentOn(Activity activity, AppUser author, string body, DateTime createdAt) =>
        new() { Activity = activity, Author = author, Body = body, CreatedAt = createdAt };

    [Fact]
    public async Task Creates_a_comment_written_by_the_current_user()
    {
        var bob = TestData.CreateUser("bob");
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), bob);
        await _db.SeedAsync(activity);

        var result = await CreateAsync(activity.Id, "Hello!", "bob");

        Assert.True(result.IsSuccess);
        Assert.Equal("Hello!", result.Value!.Body);
        Assert.Equal("bob", result.Value.Username);
        Assert.Equal("Bob", result.Value.DisplayName);

        await using var verify = _db.CreateContext();
        var saved = await verify.Comments.Include(c => c.Author).SingleAsync();
        Assert.Equal("Hello!", saved.Body);
        Assert.Equal("bob", saved.Author.UserName);
    }

    [Fact]
    public async Task Fails_for_an_unknown_activity()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await CreateAsync(Guid.NewGuid(), "Hello!", "bob");

        Assert.False(result.IsSuccess);
        await using var verify = _db.CreateContext();
        Assert.Empty(await verify.Comments.ToListAsync());
    }

    [Fact]
    public async Task Fails_for_an_unknown_user()
    {
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"));
        await _db.SeedAsync(activity);

        var result = await CreateAsync(activity.Id, "Hello!", "ghost");

        Assert.False(result.IsSuccess);
        await using var verify = _db.CreateContext();
        Assert.Empty(await verify.Comments.ToListAsync());
    }

    [Fact]
    public async Task Lists_only_the_comments_of_the_activity_newest_first()
    {
        var bob = TestData.CreateUser("bob");
        var first = TestData.CreateActivity("A", DateTime.UtcNow.AddDays(1), bob);
        var second = TestData.CreateActivity("B", DateTime.UtcNow.AddDays(2), bob);
        var now = DateTime.UtcNow;
        first.Comments.Add(CommentOn(first, bob, "oldest", now.AddMinutes(-3)));
        first.Comments.Add(CommentOn(first, bob, "newest", now.AddMinutes(-1)));
        first.Comments.Add(CommentOn(first, bob, "middle", now.AddMinutes(-2)));
        second.Comments.Add(CommentOn(second, bob, "elsewhere", now));
        await _db.SeedAsync(first, second);

        var comments = await ListAsync(first.Id);

        Assert.Equal(new[] { "newest", "middle", "oldest" }, comments.Select(c => c.Body).ToArray());
    }

    [Fact]
    public async Task Lists_at_most_the_latest_100_comments()
    {
        var bob = TestData.CreateUser("bob");
        var activity = TestData.CreateActivity("A", DateTime.UtcNow.AddDays(1), bob);
        var start = DateTime.UtcNow.AddHours(-3);
        for (var i = 0; i < 105; i++)
        {
            activity.Comments.Add(CommentOn(activity, bob, $"c{i}", start.AddMinutes(i)));
        }
        await _db.SeedAsync(activity);

        var comments = await ListAsync(activity.Id);

        Assert.Equal(100, comments.Length);
        Assert.Equal("c104", comments[0].Body);
    }
}