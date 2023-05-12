#nullable disable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Infrastructure.Logging;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Controllers;

[ApiController]
[Route("teachers/{trn}/itt-outcome")]
public class IttOutcomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public IttOutcomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("")]
    [SwaggerOperation(summary: "Sets ITT outcome for a teacher")]
    [ProducesResponseType(typeof(SetIttOutcomeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
    [RedactQueryParam("birthdate")]
    public async Task<IActionResult> SetIttOutcome([FromBody] SetIttOutcomeRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
