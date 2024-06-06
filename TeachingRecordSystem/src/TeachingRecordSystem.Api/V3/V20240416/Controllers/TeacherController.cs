using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240416.Requests;
using TeachingRecordSystem.Api.V3.V20240416.Responses;

namespace TeachingRecordSystem.Api.V3.V20240416.Controllers;

[Route("teacher")]
public class TeacherController : ControllerBase
{
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetCurrentTeacher",
        Summary = "Get the current teacher's details",
        Description = "Gets the details for the authenticated teacher.")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetTeacherRequestIncludes? include,
        [FromServices] GetPersonHandler handler)
    {
        var trn = User.FindFirstValue("trn");

        if (trn is null)
        {
            return MissingOrInvalidTrn();
        }

        var command = new GetPersonCommand(
            trn,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            DateOfBirth: null);

        var result = await handler.Handle(command);

        if (result is null)
        {
            return MissingOrInvalidTrn();
        }

        return Ok(result);

        IActionResult MissingOrInvalidTrn() => Forbid();
    }
}
