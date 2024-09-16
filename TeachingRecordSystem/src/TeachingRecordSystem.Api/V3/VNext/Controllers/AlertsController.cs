using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Api.V3.VNext.Responses;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("alerts")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = $"{ApiRoles.UpdateRole}")]
public class AlertsController : ControllerBase
{
    [HttpPost("")]
    [SwaggerOperation(
        OperationId = "CreateAlert",
        Summary = "Create an alert",
        Description = "Creates an alert for the specified person.")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateAlert([FromBody] CreateAlertRequestBody request) => throw new NotImplementedException();

    [HttpGet("{alertId}")]
    [SwaggerOperation(
        OperationId = "GetAlert",
        Summary = "Get an alert",
        Description = "Gets the specified alert.")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAlert([FromRoute] Guid alertId) => throw new NotImplementedException();

    [HttpPatch("{alertId}")]
    [SwaggerOperation(
        OperationId = "UpdateAlert",
        Summary = "Update an alert",
        Description = "Updates the specified alert.")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateAlert([FromRoute] Guid alertId, [FromBody] UpdateAlertRequestBody request) => throw new NotImplementedException();

    [HttpDelete("{alertId}")]
    [SwaggerOperation(
        OperationId = "DeleteAlert",
        Summary = "Delete an alert",
        Description = "Deletes a the specified alert.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteAlert([FromRoute] Guid alertId) => throw new NotImplementedException();
}
