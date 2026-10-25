using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    /// <summary>Display name of the user.</summary>
    [Required, StringLength(50, MinimumLength = 2)]
    public string DispayName { get; set; } = string.Empty;

    /// <summary>User email address.</summary>
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>User password (complexity is validated by ASP.NET Core Identity).</summary>
    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Unique username.</summary>
    [Required, StringLength(30, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
}