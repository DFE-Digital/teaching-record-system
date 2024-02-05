using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[ApiController]
[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.CreateTrn)]
public class TrnRequestsController(IMediator _mediator) : ControllerBase
{
    [HttpPost("")]
    [SwaggerOperation(
        OperationId = "CreateTrnRequest",
        Summary = "Creates a TRN request",
        Description = """
        Creates a new TRN request using the personally identifiable information in the request body.
        If the request can be fulfilled immediately the response's status property will be 'Completed' and a TRN will also be returned.
        Otherwise, the response's status property will be 'Pending' and the GET endpoint should be polled until a 'Completed' status is returned.
        """)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [MapError(10029, statusCode: StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTrnRequest([FromBody] CreateTrnRequestBody request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("")]
    [SwaggerOperation(
        OperationId = "GetTrnRequest",
        Summary = "Get the TRN request's details",
        Description = """
        Gets the TRN request for the requestId specified in the query string.
        If the request's status is 'Completed' a TRN will also be returned.
        """)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrnRequest([FromQuery] string requestId)
    {
        var request = new GetTrnRequest()
        {
            RequestId = Guid.Parse(requestId)
        };

        var response = await _mediator.Send(request);

        if (response is null)
        {
            return NotFound();
        }
        return Ok(response);
    }
}
