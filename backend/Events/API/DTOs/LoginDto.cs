using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class LoginDto
{
    /// <summary>
    /// User email address.
    /// </summary>
    [Required]
    [EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;
}