using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250627.Requests;
using TeachingRecordSystem.Api.V3.V20250627.Responses;

namespace TeachingRecordSystem.Api.V3.V20260120.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [HttpGet("{trn}")]    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = $"{ApiRoles.GetPerson},{ApiRoles.AppropriateBody}")]
    public async Task<IActionResult> GetAsync(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder))] GetPersonRequestIncludes? include,
        [FromQuery] DateOnly? dateOfBirth,
        [FromQuery] string? nationalInsuranceNumber)
    {
        include ??= GetPersonRequestIncludes.None;

        // For now we don't support both a DOB and NINO being passed
        if (dateOfBirth is not null && nationalInsuranceNumber is not null)
        {
            return BadRequest();
        }

        var command = new GetPersonCommand(
            trn,
            (GetPersonCommandIncludes)include,
            dateOfBirth,
            nationalInsuranceNumber,
            new GetPersonCommandOptions()
            {
                ApplyAppropriateBodyUserRestrictions = User.IsInRole(ApiRoles.AppropriateBody)
            });

        var result = await commandDispatcher.DispatchAsync(command);

        return result
            .ToActionResult(r => Ok(mapper.Map<GetPersonResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.RecordIsDeactivated, StatusCodes.Status410Gone)
            .MapErrorCode(
                ApiError.ErrorCodes.RecordIsMerged,
                e => RedirectPermanentPreserveMethod(
                    Url.Action("Get", new { trn = (string)e.Data[ApiError.DataKeys.MergedWithTrn] })!))
            .MapErrorCode(ApiError.ErrorCodes.ForbiddenForAppropriateBody, StatusCodes.Status403Forbidden);
    }
}
