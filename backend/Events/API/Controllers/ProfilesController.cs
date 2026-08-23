using System.Threading.Tasks;
using Application.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProfilesController : BaseApiController
{
    /// <summary>
    /// Gets a user's profile by username.
    /// </summary>
    /// <param name="username">The username of the profile.</param>
    [HttpGet("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(string username)
    {
        return HandleResult(await Mediator.Send(new Details.Query { Username = username }));
    }

    /// <summary>
    /// Gets activities associated with a user's profile.
    /// </summary>
    /// <param name="username">The username of the profile.</param>
    /// <param name="predicate">
    /// Specifies which activities to retrieve:
    /// "past" for past activities,
    /// "hosting" for activities hosted by the user,
    /// any other value for upcoming activities.
    /// </param>
    [HttpGet("{username}/activities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserActivities(string username, string predicate)
    {
        return HandleResult(await Mediator.Send(new ListActivities.Query { Username = username, Predicate = predicate }));
    }
}