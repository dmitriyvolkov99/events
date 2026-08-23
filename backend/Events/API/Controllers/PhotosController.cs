using System.Threading.Tasks;
using Application.Photos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PhotosController : BaseApiController
{
    /// <summary>
    /// Uploads a new photo for the current user.
    /// </summary>
    /// <param name="command">The photo file to upload.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Add([FromForm] Add.Command command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a photo belonging to the current user.
    /// </summary>
    /// <param name="id">The identifier of the photo to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
    }

    /// <summary>
    /// Sets the specified photo as the main profile photo.
    /// </summary>
    /// <param name="id">The identifier of the photo to set as the main photo.</param>
    [HttpPost("{id}/setMain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMain(string id)
    {
        return HandleResult(await Mediator.Send(new SetMain.Command { Id = id }));
    }
}