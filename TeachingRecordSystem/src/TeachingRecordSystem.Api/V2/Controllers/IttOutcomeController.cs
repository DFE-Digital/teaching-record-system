using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("teachers/{trn}/itt-outcome")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
public class IttOutcomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public IttOutcomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("")]
    [SwaggerOperation(
        OperationId = "SetIttOutcome",
        Summary = "Set ITT outcome",
        Description = "Sets the ITT outcome for a teacher")]
    [ProducesResponseType(typeof(SetIttOutcomeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetIttOutcomeAsync([FromBody] SetIttOutcomeRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
