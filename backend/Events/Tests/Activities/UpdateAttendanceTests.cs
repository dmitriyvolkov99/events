using Application.Activities;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests;

public class UpdateAttendanceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> AttendAsync(Guid activityId, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new UpdateAttendance.Handler(
            context: context,
            userAccessor: new FakeUserAccessor(username),
            logger: NullLogger<UpdateAttendance.Handler>.Instance);
        return await handler.Handle(new UpdateAttendance.Command { Id = activityId }, CancellationToken.None);
    }

    private async Task<Activity> LoadAsync(Guid id)
    {
        await using var context = _db.CreateContext();
        return await context.Activities
            .Include(a => a.Attendees)
            .ThenInclude(a => a.AppUser)
            .SingleAsync(a => a.Id == id);
    }

    private static string[] Names(Activity activity) =>
        activity.Attendees.Select(a => a.AppUser.UserName!).OrderBy(n => n).ToArray();

    [Fact]
    public async Task A_user_who_is_not_attending_joins()
    {
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"));
        await _db.SeedAsync(activity, TestData.CreateUser("anna"));

        var result = await AttendAsync(activity.Id, "anna");

        Assert.True(result.IsSuccess);
        var saved = await LoadAsync(activity.Id);
        Assert.Equal(new[] { "anna", "bob" }, Names(saved));
        Assert.False(saved.Attendees.Single(a => a.AppUser.UserName == "anna").IsHost);
    }

    [Fact]
    public async Task An_attendee_leaves()
    {
        var activity = TestData.CreateActivity(
            "Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"), TestData.CreateUser("tom"));
        await _db.SeedAsync(activity);

        var result = await AttendAsync(activity.Id, "tom");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "bob" }, Names(await LoadAsync(activity.Id)));
    }

    [Fact]
    public async Task The_host_toggles_the_cancellation_and_stays_the_host()
    {
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"));
        await _db.SeedAsync(activity);

        await AttendAsync(activity.Id, "bob");
        var cancelled = await LoadAsync(activity.Id);
        Assert.True(cancelled.IsCancelled);
        Assert.True(Assert.Single(cancelled.Attendees).IsHost);

        await AttendAsync(activity.Id, "bob");
        Assert.False((await LoadAsync(activity.Id)).IsCancelled);
    }

    [Fact]
    public async Task Joining_a_cancelled_activity_is_refused()
    {
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"));
        activity.IsCancelled = true;
        await _db.SeedAsync(activity, TestData.CreateUser("anna"));

        var result = await AttendAsync(activity.Id, "anna");

        Assert.False(result.IsSuccess);
        Assert.Equal(new[] { "bob" }, Names(await LoadAsync(activity.Id)));
    }

    [Fact]
    public async Task An_attendee_can_still_leave_a_cancelled_activity()
    {
        var activity = TestData.CreateActivity(
            "Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"), TestData.CreateUser("tom"));
        activity.IsCancelled = true;
        await _db.SeedAsync(activity);

        var result = await AttendAsync(activity.Id, "tom");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "bob" }, Names(await LoadAsync(activity.Id)));
    }

    [Fact]
    public async Task An_unknown_activity_is_not_found()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await AttendAsync(Guid.NewGuid(), "bob");

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task An_unknown_user_is_rejected()
    {
        var activity = TestData.CreateActivity("Picnic", DateTime.UtcNow.AddDays(2), TestData.CreateUser("bob"));
        await _db.SeedAsync(activity);

        var result = await AttendAsync(activity.Id, "ghost");

        Assert.False(result.IsSuccess);
        Assert.False(result.IsNotFound);
    }
}