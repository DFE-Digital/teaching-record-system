using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250203.Requests;
using TeachingRecordSystem.Api.V3.V20250203.Responses;

namespace TeachingRecordSystem.Api.V3.V20250203.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/cpd-induction")]    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetCpdInduction)]
    public async Task<IActionResult> SetCpdInductionStatusAsync(
        [FromRoute] string trn,
        [FromBody] SetCpdInductionStatusRequest request)
    {
        var command = new SetCpdInductionStatusCommand(
            trn,
            mapper.Map<InductionStatus>(request.Status),
            request.StartDate,
            request.CompletedDate,
            request.ModifiedOn.UtcDateTime);

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.StaleRequest, StatusCodes.Status409Conflict);
    }

    [HttpGet("{trn}")]    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
            .MapErrorCode(ApiError.ErrorCodes.RecordIsDeactivated, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.RecordIsMerged, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.ForbiddenForAppropriateBody, StatusCodes.Status403Forbidden);
    }

    [HttpPost("find")]    [ProducesResponseType(typeof(FindPersonsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindPersonsAsync([FromBody] FindPersonsRequest request)
    {
        var command = new FindPersonsByTrnAndDateOfBirthCommand(request.Persons.Select(p => (p.Trn, p.DateOfBirth)));
        var result = await commandDispatcher.DispatchAsync(command);
        return result.ToActionResult(r => Ok(mapper.Map<FindPersonsResponse>(r)));
    }

    [HttpGet("")]    [ProducesResponseType(typeof(FindPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindPersonsAsync(FindPersonRequest request)
    {
        var command = new FindPersonByLastNameAndDateOfBirthCommand(request.LastName!, request.DateOfBirth!.Value);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r =>
            Ok(new FindPersonResponse()
            {
                Total = r.Total,
                Query = request,
                Results = r.Items.Select(mapper.Map<FindPersonResponseResult>).AsReadOnly()
            }));
    }
}
