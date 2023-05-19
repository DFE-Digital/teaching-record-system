using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.V2.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Controllers;

[ApiController]
[Route("npq-qualifications")]
[SupportsReadOnlyMode]
public class NpqQualificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NpqQualificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    [SwaggerOperation(summary: "Set NPQ qualification for a teacher")]
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
