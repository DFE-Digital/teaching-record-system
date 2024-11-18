using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
public class TrnRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrnRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{requestId}")]
    [SwaggerOperation(
        OperationId = "GetTrnRequest",
        Summary = "Get TRN request",
        Description = "Gets a TRN request and the associated teacher's TRN")]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrnRequestAsync(GetTrnRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }

    [HttpPut("{requestId}")]
    [SwaggerOperation(
        OperationId = "GetOrCreateTrnRequest",
        Summary = "Get or create TRN request",
        Description = "Gets or creates a request for a TRN")]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> GetOrCreateTrnRequestAsync([FromBody] GetOrCreateTrnRequest request)
    {
        var response = await _mediator.Send(request);
        var statusCode = response.WasCreated ? StatusCodes.Status201Created : StatusCodes.Status200OK;
        return StatusCode(statusCode, response);
    }
}
