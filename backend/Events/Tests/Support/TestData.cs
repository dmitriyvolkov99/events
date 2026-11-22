using Domain;

namespace Tests.Support;

public static class TestData
{
    public static AppUser CreateUser(string username) => new()
    {
        UserName = username,
        NormalizedUserName = username.ToUpperInvariant(),
        Email = $"{username}@test.com",
        NormalizedEmail = $"{username}@test.com".ToUpperInvariant(),
        DisplayName = char.ToUpperInvariant(username[0]) + username[1..]
    };

    public static Photo CreatePhoto(string id, bool isMain) => new()
    {
        Id = id,
        Url = $"https://example.com/{id}.jpg",
        IsMain = isMain
    };

    /// <summary>Событие без участников (как приходит из формы).</summary>
    public static Activity NewActivity(string title = "Event", DateTime? date = null) => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        Date = date ?? DateTime.UtcNow.AddDays(7),
        Description = "Description",
        Category = "music",
        City = "City",
        Venue = "Venue"
    };

    public static Activity CreateActivity(string title, DateTime date, AppUser host, params AppUser[] attendees)
    {
        var activity = NewActivity(title, date);
        activity.Attendees.Add(new ActivityAttendee { AppUser = host, Activity = activity, IsHost = true });

        foreach (var attendee in attendees)
        {
            activity.Attendees.Add(new ActivityAttendee { AppUser = attendee, Activity = activity });
        }

        return activity;
    }

    /// <summary>bob: фото p1 (главное) и p2; tom: фото t1 (главное).</summary>
    public static Task SeedPhotoOwnersAsync(TestDb db)
    {
        var bob = CreateUser("bob");
        bob.Photos.Add(CreatePhoto("p1", true));
        bob.Photos.Add(CreatePhoto("p2", false));

        var tom = CreateUser("tom");
        tom.Photos.Add(CreatePhoto("t1", true));

        return db.SeedAsync(bob, tom);
    }
}