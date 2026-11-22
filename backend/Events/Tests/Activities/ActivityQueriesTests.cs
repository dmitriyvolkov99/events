using Application.Activities;
using Application.Core;
using Domain;
using Tests.Support;
using Xunit;

namespace Tests;

public class ActivityQueriesTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<ActivityDto>> GetDetailsAsync(Guid id, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new Details.Handler(
            context: context,
            mapper: TestMapper.Instance,
            userAccessor: new FakeUserAccessor(username));
        return await handler.Handle(new Details.Query { Id = id }, CancellationToken.None);
    }

    private async Task<PagedList<ActivityDto>> ListAsync(string username, ActivityParams param)
    {
        await using var context = _db.CreateContext();
        var handler = new List.Handler(
            context: context,
            mapper: TestMapper.Instance,
            userAccessor: new FakeUserAccessor(username));
        var result = await handler.Handle(new List.Query { Params = param }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value!;
    }

    private static string[] Titles(IEnumerable<ActivityDto> activities) => activities.Select(a => a.Title).ToArray();

    [Fact]
    public async Task Details_contain_the_host_attendees_and_follow_state_of_the_current_user()
    {
        var bob = TestData.CreateUser("bob");
        var tom = TestData.CreateUser("tom");
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), bob, tom);
        await _db.SeedAsync(activity, new UserFollowing { Observer = tom, Target = bob });

        var result = await GetDetailsAsync(activity.Id, "tom");

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("bob", dto.HostUsername);
        Assert.Equal(2, dto.Attendees.Count);

        var bobDto = dto.Attendees.Single(a => a.Username == "bob");
        Assert.True(bobDto.Following);
        Assert.Equal(1, bobDto.FollowersCount);
        Assert.False(dto.Attendees.Single(a => a.Username == "tom").Following);
    }

    [Fact]
    public async Task Details_of_an_unknown_activity_are_not_found()
    {
        var result = await GetDetailsAsync(Guid.NewGuid(), "bob");

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task List_returns_upcoming_activities_in_date_order()
    {
        var bob = TestData.CreateUser("bob");
        await _db.SeedAsync(
            TestData.CreateActivity("past", DateTime.UtcNow.AddDays(-5), bob),
            TestData.CreateActivity("later", DateTime.UtcNow.AddDays(10), bob),
            TestData.CreateActivity("soon", DateTime.UtcNow.AddDays(2), bob));

        var page = await ListAsync("bob", new ActivityParams());

        Assert.Equal(new[] { "soon", "later" }, Titles(page));
    }

    [Fact]
    public async Task List_can_be_limited_to_activities_hosted_by_the_current_user()
    {
        var bob = TestData.CreateUser("bob");
        var tom = TestData.CreateUser("tom");
        await _db.SeedAsync(
            TestData.CreateActivity("bob's", DateTime.UtcNow.AddDays(1), bob),
            TestData.CreateActivity("tom's", DateTime.UtcNow.AddDays(2), tom));

        var page = await ListAsync("bob", new ActivityParams { IsHost = true });

        Assert.Equal(new[] { "bob's" }, Titles(page));
    }

    [Fact]
    public async Task List_can_be_limited_to_activities_the_current_user_attends()
    {
        var bob = TestData.CreateUser("bob");
        var tom = TestData.CreateUser("tom");
        var anna = TestData.CreateUser("anna");
        await _db.SeedAsync(
            TestData.CreateActivity("joined", DateTime.UtcNow.AddDays(1), bob, tom),
            TestData.CreateActivity("hosted", DateTime.UtcNow.AddDays(2), tom),
            TestData.CreateActivity("other", DateTime.UtcNow.AddDays(3), anna));

        var page = await ListAsync("tom", new ActivityParams { IsGoing = true });

        Assert.Equal(new[] { "joined", "hosted" }, Titles(page));
    }

    [Fact]
    public async Task List_is_paged()
    {
        var bob = TestData.CreateUser("bob");
        await _db.SeedAsync(
            Enumerable.Range(1, 5)
                .Select(i => TestData.CreateActivity($"E{i}", DateTime.UtcNow.AddDays(i), bob))
                .ToArray());

        var page = await ListAsync("bob", new ActivityParams { PageNumber = 2, PageSize = 2 });

        Assert.Equal(new[] { "E3", "E4" }, Titles(page));
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(2, page.CurrentPage);
    }
}