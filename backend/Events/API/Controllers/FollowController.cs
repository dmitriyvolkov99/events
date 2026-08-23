using System.Threading.Tasks;
using Application.Followers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class FollowController : BaseApiController
{
    /// <summary>
    /// Follows or unfollows the specified user.
    /// </summary>
    /// <param name="username">The username of the user to follow or unfollow.</param>
    [HttpPost("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Follow(string username)
    {
        return HandleResult(await Mediator.Send(new FollowToggle.Command { TargetUsername = username }));
    }

    /// <summary>
    /// Returns the followers or following users for the specified user.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="predicate">
    /// Specifies which users to return. Accepted values: followers or following.
    /// </param>
    [HttpGet("{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFollowings(string username, string predicate)
    {
        return HandleResult(await Mediator.Send(new List.Query { Username = username, Predicate = predicate }));
    }
}