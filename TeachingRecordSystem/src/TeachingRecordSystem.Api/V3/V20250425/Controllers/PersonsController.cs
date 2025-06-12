using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Unsafe;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250425.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250425.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/professional-statuses/{reference}")]
    [SwaggerOperation(
        OperationId = "SetProfessionalStatus",
        Summary = "Sets a professional status",
        Description = "Sets a professional status for the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetProfessionalStatus)]
    public async Task<IActionResult> SetProfessionalStatusAsync(
        [FromRoute] string trn,
        [FromRoute(Name = "reference")] string sourceApplicationReference,
        [FromBody] SetProfessionalStatusRequest request,
        [FromServices] SetRouteToProfessionalStatusHandler handler)
    {
        var command = new SetRouteToProfessionalStatusCommand(
            trn,
            sourceApplicationReference,
            request.RouteTypeId,
            mapper.Map<Implementation.Dtos.ProfessionalStatusStatus>(request.Status),
            request.AwardedDate,
            request.TrainingStartDate,
            request.TrainingEndDate,
            request.TrainingSubjectReferences.HasValue ? request.TrainingSubjectReferences.ValueOrDefault() : null,
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

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [HttpPut("{trn}")]
    [SwaggerOperation(
        OperationId = "Set PII",
        Summary = "Set a persons PII",
        Description = "Sets a persons personally identifiable information with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
    public async Task<IActionResult> SetPiiAsync(
        [FromRoute] string trn,
        [FromBody] SetPiiRequest request,
        [FromServices] SetPiiHandler handler)
    {
        var command = new SetPiiCommand()
        {
            Trn = trn,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            EmailAddresses = request.EmailAddress,
            NationalInsuranceNumber = request.NationalInsuranceNumber,
            Gender = request.Gender is Gender gender ? mapper.Map<Implementation.Dtos.Gender>(gender) : null,
        };

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
