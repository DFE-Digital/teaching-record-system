using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[ApiController]
[Route("trn-requests")]
public class TrnRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrnRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{requestId}")]
    [OpenApiOperation(
        operationId: "GetTrnRequest",
        summary: "Get TRN request",
        description: "Gets a TRN request and the associated teacher's TRN")]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrnRequest(GetTrnRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }

    [HttpPut("{requestId}")]
    [OpenApiOperation(
        operationId: "GetOrCreateTrnRequest",
        summary: "Get or create TRN request",
        description: "Gets or creates a request for a TRN")]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrCreateTrnRequest([FromBody] GetOrCreateTrnRequest request)
    {
        var response = await _mediator.Send(request);
        var statusCode = response.WasCreated ? StatusCodes.Status201Created : StatusCodes.Status200OK;
        return StatusCode(statusCode, response);
    }
}
