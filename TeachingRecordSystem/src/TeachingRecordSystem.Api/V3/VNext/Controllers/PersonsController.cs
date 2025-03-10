using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("persons")]
public class PersonsController : ControllerBase
{
    [HttpPut("{trn}/welsh-induction")]
    [SwaggerOperation(
        OperationId = "SetPersonWelshInductionStatus",
        Summary = "Set person induction status",
        Description = "Sets the induction details of the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetWelshInduction)]
    public async Task<IActionResult> SetWelshInductionStatusAsync(
        [FromRoute] string trn,
        [FromBody] SetWelshInductionStatusRequest request,
        [FromServices] SetWelshInductionStatusHandler handler)
    {
        var command = new SetWelshInductionStatusCommand(
            trn,
            request.Passed,
            request.StartDate,
            request.CompletedDate);

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [HttpPut("{trn}/professional-statuses/{id}")]
    [SwaggerOperation(
        OperationId = "SetProfessionalStatus",
        Summary = "Sets a professional status")]
    //[ProducesResponseType(typeof(FindPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetProfessionalStatus)]
    public IActionResult SetProfessionalStatus(
        [FromRoute] string id,
        [FromBody] SetProfessionalStatusRequest request) =>
        NoContent();


    [HttpPut("{trn}/set-pii")]
    [SwaggerOperation(
        OperationId = "Set PII",
        Summary = "Set a persons PII",
        Description = "Sets a persons personally identifiable information with the given TRN.")]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
    public async Task<IActionResult> SetPIIAsync(
        [FromRoute] string trn,
        [FromBody] SetPIIRequest request,
        [FromServices] SetPIIHandler handler)
    {
        var command = new SetPIICommand()
        {
            Trn = trn,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            EmailAddresses = request.EmailAddresses,
            NationalInsuranceNumber = request.NationalInsuranceNumber,
            Gender = Gender.Male
        };

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
