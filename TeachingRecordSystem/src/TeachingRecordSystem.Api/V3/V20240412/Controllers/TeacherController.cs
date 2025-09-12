using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240412.Requests;
using TeachingRecordSystem.Api.V3.V20240412.Responses;

namespace TeachingRecordSystem.Api.V3.V20240412.Controllers;

[Route("teacher")]
public class TeacherController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [HttpPost("name-changes")]
    [SwaggerOperation(
        OperationId = "CreateNameChange",
        Summary = "Create name change request",
        Description = "Creates a name change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateNameChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateNameChangeAsync([FromBody] CreateNameChangeRequestRequest request)
    {
        var command = new CreateNameChangeRequestCommand()
        {
            Trn = User.FindFirstValue("trn")!,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = request.Email
        };

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<CreateNameChangeResponse>(r)));
    }

    [HttpPost("date-of-birth-changes")]
    [SwaggerOperation(
        OperationId = "CreateDobChange",
        Summary = "Create DOB change request",
        Description = "Creates a date of birth change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateDateOfBirthChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateDateOfBirthChangeAsync([FromBody] CreateDateOfBirthChangeRequestRequest request)
    {
        var command = new CreateDateOfBirthChangeRequestCommand()
        {
            Trn = User.FindFirstValue("trn")!,
            DateOfBirth = request.DateOfBirth,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = request.Email
        };

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<CreateDateOfBirthChangeResponse>(r)));
    }
}
