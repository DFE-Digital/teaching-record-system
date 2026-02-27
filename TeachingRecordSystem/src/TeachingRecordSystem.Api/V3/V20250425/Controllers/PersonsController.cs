using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Unsafe;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250425.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;
using Gender = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.Gender;

namespace TeachingRecordSystem.Api.V3.V20250425.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/professional-statuses/{reference}")]
    [EndpointName("SetProfessionalStatus"),
        EndpointSummary("Sets a professional status"),
        EndpointDescription("Sets a professional status for the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetProfessionalStatus)]
    public async Task<IActionResult> SetProfessionalStatusAsync(
        [FromRoute] string trn,
        [FromRoute(Name = "reference")] string sourceApplicationReference,
        [FromBody] SetProfessionalStatusRequest request)
    {
        var command = new SetRouteToProfessionalStatusCommand(
            trn,
            sourceApplicationReference,
            request.RouteTypeId,
            request.Status.ConvertToRouteToProfessionalStatusStatus(),
            request.AwardedDate,
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

    [HttpPut("{trn}")]
    [EndpointName("Set PII"),
        EndpointSummary("Set a persons PII"),
        EndpointDescription("Sets a persons personally identifiable information with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
    public async Task<IActionResult> SetPiiAsync(
        [FromRoute] string trn,
        [FromBody] SetPiiRequest request)
    {
        var command = new SetPiiCommand()
        {
            Trn = trn,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            EmailAddress = request.EmailAddress,
            NationalInsuranceNumber = request.NationalInsuranceNumber,
            Gender = request.Gender is Gender gender ? mapper.Map<Core.Models.Gender>(gender) : null
        };

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
