using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user == null)
        {
            _logger.LogWarning(
                "Login attempt failed: user with email {Email} was not found",
                loginDto.Email);

            return NotFound();
        }

        var result = await _signManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "User {Username} logged in successfully",
                user.UserName);

            return CreateUserObject(user);
        }

        _logger.LogWarning(
            "Login attempt failed for user {Username}",
            user.UserName);

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
                "Registration attempt failed: email {Email} is already registered",
                registerDto.Email);

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
            _logger.LogInformation(
                            "User {Username} registered successfully",
                            user.UserName);

            return CreateUserObject(user);
        }

        _logger.LogError(
           "Failed to register user {Username}. Identity errors: {Errors}",
           user.UserName,
           string.Join("; ", result.Errors.Select(e => e.Description)));

        return BadRequest("Problem registering user");
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
            return NotFound();

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