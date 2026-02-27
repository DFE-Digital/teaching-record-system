using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240416.Requests;
using TeachingRecordSystem.Api.V3.V20240416.Responses;

namespace TeachingRecordSystem.Api.V3.V20240416.Controllers;

[Route("teacher")]
public class TeacherController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetCurrentTeacher",
        Summary = "Get the current teacher's details",
        Description = "Gets the details for the authenticated teacher.")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetTeacherRequestIncludes? include)
    {
        var command = new GetPersonCommand(
            Trn: User.FindFirstValue("trn")!,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            DateOfBirth: null,
            NationalInsuranceNumber: null,
            new GetPersonCommandOptions()
            {
                ApplyLegacyAlertsBehavior = true
            });

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<GetTeacherResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status403Forbidden);
    }
}
