using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class LoginDto
{
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
    public string Password { get; set; } = string.Empty;
}