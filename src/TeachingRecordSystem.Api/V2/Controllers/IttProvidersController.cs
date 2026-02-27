using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("itt-providers")]
[Authorize(Policy = AuthorizationPolicies.ApiKey)]
public class IttProvidersController(IMediator mediator) : ControllerBase
{
    [HttpGet("")]
    [EndpointName("GetIttProviders"),
        EndpointSummary("Get ITT Providers"),
        EndpointDescription("Gets the list of ITT providers")]
    [ProducesResponseType(typeof(GetIttProvidersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIttProvidersAsync()
    {
        var request = new GetIttProvidersRequest();
        var response = await mediator.Send(request);
        return Ok(response);
    }
}
