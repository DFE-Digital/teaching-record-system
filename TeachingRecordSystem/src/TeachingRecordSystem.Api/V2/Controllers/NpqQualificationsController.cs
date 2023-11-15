using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;

namespace TeachingRecordSystem.Api.V2.Controllers;

[ApiController]
[Route("npq-qualifications")]
[Authorize(Policy = AuthorizationPolicies.UpdateNpq)]
public class NpqQualificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NpqQualificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    [OpenApiOperation(
        operationId: "SetNpqQualification",
        summary: "Set NPQ qualification",
        description: "Sets the NPQ qualification for a teacher and qualification type")]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetNpqQualification([FromBody] SetNpqQualificationRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
