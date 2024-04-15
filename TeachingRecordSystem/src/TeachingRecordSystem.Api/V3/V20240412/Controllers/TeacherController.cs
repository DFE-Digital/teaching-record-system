using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240412.Requests;
using TeachingRecordSystem.Api.V3.V20240412.Responses;

namespace TeachingRecordSystem.Api.V3.V20240412.Controllers;

[Route("teacher")]
public class TeacherController : ControllerBase
{
    [HttpPost("name-changes")]
    [SwaggerOperation(
        OperationId = "CreateNameChange",
        Summary = "Create name change request",
        Description = "Creates a name change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateNameChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateNameChange(
        [FromBody] CreateNameChangeRequestRequest request,
        [FromServices] CreateNameChangeRequestHandler handler)
    {
        var command = request.Adapt<CreateNameChangeRequestCommand>() with { Trn = User.FindFirstValue("trn")! };
        var caseNumber = await handler.Handle(command);
        var response = new CreateNameChangeResponse() { CaseNumber = caseNumber };
        return Ok(response);
    }

    [HttpPost("date-of-birth-changes")]
    [SwaggerOperation(
        OperationId = "CreateDobChange",
        Summary = "Create DOB change request",
        Description = "Creates a date of birth change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateDateOfBirthChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateDateOfBirthChange(
        [FromBody] CreateDateOfBirthChangeRequestRequest request,
        [FromServices] CreateDateOfBirthChangeRequestHandler handler)
    {
        var command = request.Adapt<CreateDateOfBirthChangeRequestCommand>() with { Trn = User.FindFirstValue("trn")! };
        var caseNumber = await handler.Handle(command);
        var response = new CreateNameChangeResponse() { CaseNumber = caseNumber };
        return Ok(response);
    }
}
