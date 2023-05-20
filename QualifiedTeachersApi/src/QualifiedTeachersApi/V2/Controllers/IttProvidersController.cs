using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Controllers;

[ApiController]
[Route("itt-providers")]
[SupportsReadOnlyMode]
public class IttProvidersController : ControllerBase
{
    private readonly IMediator _mediator;

    public IttProvidersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [OpenApiOperation(
        operationId: "GetIttProviders",
        summary: "Get ITT Providers",
        description: "Gets the list of ITT providers")]
    [ProducesResponseType(typeof(GetIttProvidersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIttProviders()
    {
        var request = new GetIttProvidersRequest();
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
