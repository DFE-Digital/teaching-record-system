using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Api.V3.VNext.Response;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("alerts")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = $"{ApiRoles.UpdateRole}")]
public class AlertsController : ControllerBase
{
    [HttpPost("")]
    [EndpointName("CreateAlert"),
        EndpointSummary("Create an alert"),
        EndpointDescription("Creates an alert for the specified person.")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateAlert([FromBody] CreateAlertRequestBody request) => throw new NotImplementedException();

    [HttpGet("{alertId}")]
    [EndpointName("GetAlert"),
        EndpointSummary("Get an alert"),
        EndpointDescription("Gets the specified alert.")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAlert([FromRoute] Guid alertId) => throw new NotImplementedException();

    [HttpPatch("{alertId}")]
    [EndpointName("UpdateAlert"),
        EndpointSummary("Update an alert"),
        EndpointDescription("Updates the specified alert.")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateAlert([FromRoute] Guid alertId, [FromBody] UpdateAlertRequestBody request) => throw new NotImplementedException();

    [HttpDelete("{alertId}")]
    [EndpointName("DeleteAlert"),
        EndpointSummary("Delete an alert"),
        EndpointDescription("Deletes a the specified alert.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteAlert([FromRoute] Guid alertId) => throw new NotImplementedException();
}
