using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerOperation(summary: "Gets a list of all ITT Providers")]
    [ProducesResponseType(typeof(GetIttProvidersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIttProviders()
    {
        var request = new GetIttProvidersRequest();
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
