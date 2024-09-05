using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.VNext.ApiModels;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Api.V3.VNext.Responses;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "SetQtls",
        Summary = "Set QTLS status for a teacher",
        Description = "Sets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status202Accepted)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> PutQtls(
        [FromRoute] string trn,
        [FromBody] SetQtlsRequest request,
        [FromServices] SetQtlsHandler handler)
    {
        var command = new SetQtlsCommand(trn, request.QtsDate);
        var result = await handler.Handle(command);
        return result is { Succeeded: true } ? Ok(result.QtlsInfo!) : Accepted();
    }

    [HttpGet("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "GetQtls",
        Summary = "Get QTLS status for a teacher",
        Description = "Gets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsInfo), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> GetQtls(
        [FromRoute] string trn,
        [FromServices] GetQtlsHandler handler)
    {
        var command = new GetQtlsCommand(trn);
        var result = await handler.Handle(command);
        var response = mapper.Map<QtlsInfo?>(result);
        return response is not null ? Ok(response) : NotFound();
    }

    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetPersonByTrn",
        Summary = "Get person details by TRN",
        Description = "Gets the details of the person corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = $"{ApiRoles.GetPerson},{ApiRoles.AppropriateBody}")]
    public async Task<IActionResult> Get(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetPersonRequestIncludes? include,
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified dateOfBirth, if provided.")] DateOnly? dateOfBirth,
        [FromServices] GetPersonHandler handler)
    {
        include ??= GetPersonRequestIncludes.None;

        if (User.IsInRole(ApiRoles.AppropriateBody))
        {
            if ((include & ~(GetPersonRequestIncludes.Induction | GetPersonRequestIncludes.Alerts)) != 0)
            {
                return Forbid();
            }

            if (dateOfBirth is null)
            {
                return Forbid();
            }
        }

        var command = new GetPersonCommand(
            trn,
            (GetPersonCommandIncludes)include,
            dateOfBirth);

        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = mapper.Map<GetPersonResponse>(result);
        return Ok(response);
    }
}
