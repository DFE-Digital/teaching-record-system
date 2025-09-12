using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Unsafe;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250627.Requests;
using TeachingRecordSystem.Api.V3.V20250627.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250627.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
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
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified nationalInsuranceNumber, if provided.")] string? nationalInsuranceNumber)
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
    public async Task<IActionResult> FindPersonsAsync([FromBody] FindPersonsRequest request)
    {
        var command = new FindPersonsByTrnAndDateOfBirthCommand(request.Persons.Select(p => (p.Trn, p.DateOfBirth)));
        var result = await commandDispatcher.DispatchAsync(command);
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

    [HttpPut("{trn}/professional-statuses/{reference}")]
    [RemovesFromApi]
    public IActionResult SetProfessionalStatus() => throw null!;

    [HttpPut("{trn}/routes-to-professional-statuses/{reference}")]
    [SwaggerOperation(
        OperationId = "SetRouteToProfessionalStatus",
        Summary = "Sets a route to professional status",
        Description = "Sets a route to professional status for the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetProfessionalStatus)]
    public async Task<IActionResult> SetRouteToProfessionalStatusAsync(
        [FromRoute] string trn,
        [FromRoute(Name = "reference")] string sourceApplicationReference,
        [FromBody] SetRouteToProfessionalStatusRequest request)
    {
        var command = new SetRouteToProfessionalStatusCommand(
            trn,
            sourceApplicationReference,
            request.RouteToProfessionalStatusTypeId,
            mapper.Map<RouteToProfessionalStatusStatus>(request.Status),
            request.HoldsFrom,
            request.TrainingStartDate,
            request.TrainingEndDate,
            request.TrainingSubjectReferences.HasValue ? request.TrainingSubjectReferences.ValueOrDefault() : [],
            request.TrainingAgeSpecialism is null
                ? null
                : new SetRouteToProfessionalStatusCommandTrainingAgeSpecialism(
                    request.TrainingAgeSpecialism.Type.ConvertToTrainingAgeSpecialismType(),
                    request.TrainingAgeSpecialism.From,
                    request.TrainingAgeSpecialism.To),
            request.TrainingCountryReference,
            request.TrainingProviderUkprn,
            request.DegreeTypeId,
            request.IsExemptFromInduction);

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
