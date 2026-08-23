using System;
using System.Threading.Tasks;
using Application.Activities;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ActivitiesController : BaseApiController
{
    /// <summary>
    /// Returns a paginated list of activities.
    /// </summary>
    /// <param name="param">Filtering and pagination parameters.</param>
    /// <returns>A paginated list of activities.</returns>
    [HttpGet]
    public async Task<IActionResult> GetActivities([FromQuery] ActivityParams param)
    {
        return HandlePagedResult(await Mediator.Send(new List.Query { Params = param }));
    }

    /// <summary>
    /// Returns an activity by its identifier.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <returns>The requested activity.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetActivity(Guid id)
    {
        return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="activity">Activity data.</param>
    /// <returns>The created activity.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateActivity(Activity activity)
    {
        return HandleResult(await Mediator.Send(new Create.Command { Activity = activity }));
    }

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="activity">Updated activity data.</param>
    [Authorize(Policy = "isActivityHost")]
    [HttpPut("{id}")]
    public async Task<IActionResult> EditActivity(Guid id, Activity activity)
    {
        activity.Id = id;
        return HandleResult(await Mediator.Send(new Edit.Command { Activity = activity }));
    }

    /// <summary>
    /// Deletes an activity.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
    }

    /// <summary>
    /// Adds or removes the current user from an activity.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    [HttpPost("{id}/attend")]
    public async Task<IActionResult> Attend(Guid id)
    {
        return HandleResult(await Mediator.Send(new UpdateAttendance.Command { Id = id }));
    }
}