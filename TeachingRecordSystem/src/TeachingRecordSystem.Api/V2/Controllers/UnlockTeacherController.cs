using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("unlock-teacher")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UnlockPerson)]
public class UnlockTeacherController : ControllerBase
{
    [HttpPut("{teacherId}")]
    [EndpointName("UnlockTeacher"),
        EndpointSummary("Unlock teacher"),
        EndpointDescription("Unlocks the teacher record allowing the teacher to sign in to the portals")]
    [ProducesResponseType(typeof(UnlockTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public IActionResult UnlockTeacher() => Ok(new UnlockTeacherResponse { HasBeenUnlocked = false });
}
