using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("teachers")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("find")]
    [EndpointName("FindTeachers"),
        EndpointSummary("Find teachers"),
        EndpointDescription("Returns teachers matching the specified criteria")]
    [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindTeachersAsync(FindTeachersRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{trn}")]
    [EndpointName("GetTeacher"),
        EndpointSummary("Get teacher"),
        EndpointDescription("Gets an individual teacher by their TRN")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> GetTeacherAsync([FromRoute] GetTeacherRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }
}
