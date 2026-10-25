using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signManager;
    private readonly TokenService _tokenService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signManager,
        TokenService tokenService,
        ILogger<AccountController> logger)
    {
        _tokenService = tokenService;
        _signManager = signManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <param name="loginDto">User login credentials.</param>
    /// <returns>The authenticated user with a JWT token.</returns>
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var normalizedEmail = _userManager.NormalizeEmail(loginDto.Email);
        var user = await _userManager.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: unknown email");
            return Unauthorized();
        }

        var result = await _signManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Username} logged in successfully", user.UserName);
            return CreateUserObject(user);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login blocked: user {Username} is locked out", user.UserName);
            return StatusCode(StatusCodes.Status429TooManyRequests, "Too many failed attempts. Try again later.");
        }

        _logger.LogWarning("Login attempt failed for user {Username}", user.UserName);
        return Unauthorized();
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="registerDto">User registration data.</param>
    /// <returns>The newly registered user with a JWT token.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
        {
            _logger.LogWarning(
                "Registration attempt failed: email is already registered");

            ModelState.AddModelError("email", "Email taken");
            return ValidationProblem();
        }

        if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
        {
            _logger.LogWarning(
                "Registration attempt failed: username {Username} is already taken",
                registerDto.Username);

            ModelState.AddModelError("username", "Username taken");
            return ValidationProblem();
        }

        var user = new AppUser
        {
            DispayName = registerDto.DispayName,
            Email = registerDto.Email,
            UserName = registerDto.Username
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Username} registered successfully", user.UserName);
            return CreateUserObject(user);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        _logger.LogWarning(
            "Registration failed for user {Username}: {Codes}",
            user.UserName,
            string.Join(", ", result.Errors.Select(e => e.Code)));

        return ValidationProblem();
    }

    /// <summary>
    /// Returns the currently authenticated user.
    /// </summary>
    /// <returns>The currently authenticated user with a JWT token.</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userManager.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

        if (user == null)
        {
            return Unauthorized();
        }

        return CreateUserObject(user);
    }


    private UserDto CreateUserObject(AppUser user)
    {
        return new UserDto
        {
            DispayName = user.DispayName,
            Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            Token = _tokenService.CreateToken(user),
            Username = user.UserName!
        };
    }
}