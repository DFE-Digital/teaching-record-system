using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("itt-providers")]
[Authorize(Policy = AuthorizationPolicies.ApiKey)]
public class IttProvidersController : ControllerBase
{
    private readonly IMediator _mediator;

    public IttProvidersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]    [ProducesResponseType(typeof(GetIttProvidersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIttProvidersAsync()
    {
        var request = new GetIttProvidersRequest();
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}
