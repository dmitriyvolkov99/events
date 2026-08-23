using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    /// <summary>
    /// Display name of the user.
    /// </summary>
    [Required]
    public string DispayName { get; set; } = string.Empty;

    /// <summary>
    /// User email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    [Required]
    [RegularExpression(
        "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$",
        ErrorMessage = "Password must be complex")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Unique username.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;
}