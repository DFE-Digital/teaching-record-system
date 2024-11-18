using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("unlock-teacher")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UnlockPerson)]
public class UnlockTeacherController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnlockTeacherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{teacherId}")]
    [SwaggerOperation(
        OperationId = "UnlockTeacher",
        Summary = "Unlock teacher",
        Description = "Unlocks the teacher record allowing the teacher to sign in to the portals")]
    [ProducesResponseType(typeof(UnlockTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockTeacherAsync([FromRoute] UnlockTeacherRequest request)
    {
        try
        {
            return Ok(await _mediator.Send(request));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
