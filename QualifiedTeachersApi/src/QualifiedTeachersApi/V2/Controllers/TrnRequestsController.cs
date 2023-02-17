using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Controllers;

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
    [SwaggerOperation(summary: "Retrieves a TRN request")]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrnRequest(GetTrnRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }

    [HttpPut("{requestId}")]
    [SwaggerOperation(summary: "Creates a request for a TRN")]
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
