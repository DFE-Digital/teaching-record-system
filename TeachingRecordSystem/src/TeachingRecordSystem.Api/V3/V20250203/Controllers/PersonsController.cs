using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250203.Requests;
using TeachingRecordSystem.Api.V3.V20250203.Responses;

namespace TeachingRecordSystem.Api.V3.V20250203.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/cpd-induction")]
    [SwaggerOperation(
        OperationId = "SetPersonCpdInductionStatus",
        Summary = "Set person induction status",
        Description = "Sets the induction details of the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetCpdInduction)]
    public async Task<IActionResult> SetCpdInductionStatusAsync(
        [FromRoute] string trn,
        [FromBody] SetCpdInductionStatusRequest request,
        [FromServices] SetCpdInductionStatusHandler handler)
    {
        var command = new SetCpdInductionStatusCommand(
            trn,
            mapper.Map<InductionStatus>(request.Status),
            request.StartDate,
            request.CompletedDate,
            request.ModifiedOn.UtcDateTime);

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.StaleRequest, StatusCodes.Status409Conflict);
    }

    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetPersonByTrn",
        Summary = "Get person details by TRN",
        Description = "Gets the details of the person corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = $"{ApiRoles.GetPerson},{ApiRoles.AppropriateBody}")]
    public async Task<IActionResult> GetAsync(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetPersonRequestIncludes? include,
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified dateOfBirth, if provided.")] DateOnly? dateOfBirth,
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified nationalInsuranceNumber, if provided.")] string? nationalInsuranceNumber,
        [FromServices] GetPersonHandler handler)
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

        var result = await handler.HandleAsync(command);

        return result
            .ToActionResult(r => Ok(mapper.Map<GetPersonResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.ForbiddenForAppropriateBody, StatusCodes.Status403Forbidden);
    }

    [HttpPost("find")]
    [SwaggerOperation(
        OperationId = "FindPersons",
        Summary = "Find persons",
        Description = "Finds persons matching the specified criteria.")]
    [ProducesResponseType(typeof(FindPersonsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindPersonsAsync(
        [FromBody] FindPersonsRequest request,
        [FromServices] FindPersonsByTrnAndDateOfBirthHandler handler)
    {
        var command = new FindPersonsByTrnAndDateOfBirthCommand(request.Persons.Select(p => (p.Trn, p.DateOfBirth)));
        var result = await handler.HandleAsync(command);
        return result.ToActionResult(r => Ok(mapper.Map<FindPersonsResponse>(r)));
    }

    [HttpGet("")]
    [SwaggerOperation(
        OperationId = "FindPerson",
        Summary = "Find person",
        Description = "Finds a person matching the specified criteria.")]
    [ProducesResponseType(typeof(FindPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindPersonsAsync(
        FindPersonRequest request,
        [FromServices] FindPersonByLastNameAndDateOfBirthHandler handler)
    {
        var command = new FindPersonByLastNameAndDateOfBirthCommand(request.LastName!, request.DateOfBirth!.Value);
        var result = await handler.HandleAsync(command);

        return result.ToActionResult(r =>
            Ok(new FindPersonResponse()
            {
                Total = r.Total,
                Query = request,
                Results = r.Items.Select(mapper.Map<FindPersonResponseResult>).AsReadOnly()
            }));
    }
}
