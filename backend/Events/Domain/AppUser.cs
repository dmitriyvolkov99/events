using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Domain;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }

    public ICollection<ActivityAttendee> Activities { get; set; } = [];
    public ICollection<Photo> Photos { get; set; } = [];

    public ICollection<UserFollowing> Followings { get; set; } = [];
    public ICollection<UserFollowing> Followers { get; set; } = [];
}