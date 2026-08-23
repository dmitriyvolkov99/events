using Application.Activities;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Activities;

public class DeleteActivityTests
{
    private readonly Mock<IUserAccessor> _userAccessorMock;

    public DeleteActivityTests()
    {
        _userAccessorMock = new Mock<IUserAccessor>();

    }

    [Fact]
    public async Task Handle_ShouldFail_WhenActivityDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();

        var logger = NullLogger<Delete.Handler>.Instance;

        var handler = new Delete.Handler(
            context,
            _userAccessorMock.Object,
            logger);

        var command = new Delete.Command
        {
            Id = Guid.NewGuid()
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Activity not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldDeleteActivity_WhenActivityExists()
    {
        await using var context = TestDbContextFactory.Create();

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test Activity",
            Description = "Test Description",
            Date = DateTime.UtcNow,
            Category = "Test",
            City = "Vilnius",
            Venue = "Test Venue"
        };

        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var logger = NullLogger<Delete.Handler>.Instance;

        var handler = new Delete.Handler(context, _userAccessorMock.Object, logger);

        var command = new Delete.Command
        {
            Id = activity.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var deletedActivity = await context.Activities
            .FindAsync(activity.Id);

        Assert.Null(deletedActivity);
    }
}