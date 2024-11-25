using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240920.Requests;
using TeachingRecordSystem.Api.V3.V20240920.Responses;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/induction")]
    [SwaggerOperation(
        OperationId = "SetPersonInductionStatus",
        Summary = "Set person induction status",
        Description = "Sets the induction details of the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetInduction)]
    public IActionResult SetInductionStatus([FromRoute] string trn, [FromBody] SetInductionStatusRequest request) =>
        NoContent();

    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetPersonByTrn",
        Summary = "Get person details by TRN",
        Description = "Gets the details of the person corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = $"{ApiRoles.GetPerson},{ApiRoles.AppropriateBody}")]
    public async Task<IActionResult> GetAsync(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetPersonRequestIncludes? include,
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified dateOfBirth, if provided.")] DateOnly? dateOfBirth,
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified nationalInsuranceNumber, if provided.")] string? nationalInsuranceNumber,
        [FromServices] GetPersonHandler handler)
    {
        include ??= GetPersonRequestIncludes.None;

        if (User.IsInRole(ApiRoles.AppropriateBody))
        {
            if ((include & ~(GetPersonRequestIncludes.Induction | GetPersonRequestIncludes.Alerts | GetPersonRequestIncludes.InitialTeacherTraining)) != 0)
            {
                return Forbid();
            }

            if (dateOfBirth is null || nationalInsuranceNumber is not null)
            {
                return Forbid();
            }
        }

        // For now we don't support both a DOB and NINO being passed
        if (dateOfBirth is not null && nationalInsuranceNumber is not null)
        {
            return BadRequest();
        }

        var command = new GetPersonCommand(
            trn,
            (GetPersonCommandIncludes)include,
            dateOfBirth,
            ApplyLegacyAlertsBehavior: false,
            nationalInsuranceNumber);

        var result = await handler.HandleAsync(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = GetPersonResponse.Map(result, mapper, User.IsInRole(ApiRoles.AppropriateBody));
        return Ok(response);
    }
}
