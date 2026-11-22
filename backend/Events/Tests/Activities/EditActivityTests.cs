using Application.Activities;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Support;
using Xunit;

namespace Tests;

public class EditActivityTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<Result<Unit>> EditAsync(Activity edited, string username)
    {
        await using var context = _db.CreateContext();
        var handler = new Edit.Handler(
            context: context,
            mapper: TestMapper.Instance,
            userAccessor: new FakeUserAccessor(username),
            logger: NullLogger<Edit.Handler>.Instance);
        return await handler.Handle(new Edit.Command { Activity = edited }, CancellationToken.None);
    }

    [Fact]
    public async Task Updates_the_form_fields_but_keeps_attendees_and_the_cancellation_flag()
    {
        var original = TestData.CreateActivity(
            "Old title", DateTime.UtcNow.AddDays(3), TestData.CreateUser("bob"), TestData.CreateUser("tom"));
        original.IsCancelled = true;
        await _db.SeedAsync(original);

        var edited = TestData.NewActivity("New title", DateTime.UtcNow.AddDays(10));
        edited.Id = original.Id;
        edited.City = "Paris";
        edited.Category = "food";

        var result = await EditAsync(edited, "bob");

        Assert.True(result.IsSuccess);
        await using var verify = _db.CreateContext();
        var saved = await verify.Activities.Include(a => a.Attendees).SingleAsync();
        Assert.Equal("New title", saved.Title);
        Assert.Equal("Paris", saved.City);
        Assert.Equal("food", saved.Category);
        Assert.Equal(edited.Date, saved.Date);
        Assert.True(saved.IsCancelled);
        Assert.Equal(2, saved.Attendees.Count);
    }

    [Fact]
    public async Task Returns_not_found_for_an_unknown_activity()
    {
        await _db.SeedAsync(TestData.CreateUser("bob"));

        var result = await EditAsync(TestData.NewActivity(), "bob");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Saving_unchanged_values_is_a_success()
    {
        await _db.SeedAsync(TestData.CreateActivity("Event", DateTime.UtcNow.AddDays(3), TestData.CreateUser("bob")));

        Activity same;
        await using (var read = _db.CreateContext())
        {
            var saved = await read.Activities.SingleAsync();
            same = new Activity
            {
                Id = saved.Id,
                Title = saved.Title,
                Date = saved.Date,
                Description = saved.Description,
                Category = saved.Category,
                City = saved.City,
                Venue = saved.Venue
            };
        }

        var result = await EditAsync(same, "bob");

        Assert.True(result.IsSuccess);
    }
}