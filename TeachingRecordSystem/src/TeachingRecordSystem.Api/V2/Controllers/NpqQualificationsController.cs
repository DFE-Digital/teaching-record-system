using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("npq-qualifications")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdateNpq)]
public class NpqQualificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NpqQualificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    [SwaggerOperation(
        OperationId = "SetNpqQualification",
        Summary = "Set NPQ qualification",
        Description = "Sets the NPQ qualification for a teacher and qualification type")]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetNpqQualificationAsync([FromBody] SetNpqQualificationRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
