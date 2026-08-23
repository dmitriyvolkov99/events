namespace API.DTOs;

public class UserDto
{
    /// <summary>
    /// User display name.
    /// </summary>
    public string DispayName { get; set; } = string.Empty;

    /// <summary>
    /// JWT access token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Unique username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// URL of the user's main profile image.
    /// </summary>
    public string? Image { get; set; }
}