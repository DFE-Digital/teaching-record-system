using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240920.Requests;
using TeachingRecordSystem.Api.V3.V20240920.Responses;

namespace TeachingRecordSystem.Api.V3.V20240920.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
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
            if ((include & ~(GetPersonRequestIncludes.Induction | GetPersonRequestIncludes.Alerts | GetPersonRequestIncludes.InitialTeacherTraining)) != 0)
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
            dateOfBirth,
            ApplyLegacyAlertsBehavior: false);

        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = GetPersonResponse.Map(result, mapper, User.IsInRole(ApiRoles.AppropriateBody));
        return Ok(response);
    }

    [HttpPost("find")]
    [SwaggerOperation(
        OperationId = "FindPersons",
        Summary = "Find persons",
        Description = "Finds persons matching the specified criteria.")]
    [ProducesResponseType(typeof(FindPersonsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindPersons(
        [FromBody] FindPersonsRequest request,
        [FromServices] FindPersonsByTrnAndDateOfBirthHandler handler)
    {
        var command = new FindPersonsByTrnAndDateOfBirthCommand(request.Persons.Select(p => (p.Trn, p.DateOfBirth)));
        var result = await handler.Handle(command);
        var response = mapper.Map<FindPersonsResponse>(result);
        return Ok(response);
    }

    [HttpGet("")]
    [SwaggerOperation(
        OperationId = "FindPerson",
        Summary = "Find person",
        Description = "Finds a person matching the specified criteria.")]
    [ProducesResponseType(typeof(FindPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindPersons(
        FindPersonRequest request,
        [FromServices] FindPersonByLastNameAndDateOfBirthHandler handler)
    {
        var command = new FindPersonByLastNameAndDateOfBirthCommand(request.LastName!, request.DateOfBirth!.Value);
        var result = await handler.Handle(command);

        var response = new FindPersonResponse()
        {
            Total = result.Total,
            Query = request,
            Results = result.Items.Select(mapper.Map<FindPersonResponseResult>).AsReadOnly()
        };

        return Ok(response);
    }

    [HttpPut("deceased/{trn}")]
    [SwaggerOperation(
        OperationId = "SetDeceased",
        Summary = "Mark person as deceased",
        Description = "Marks a person as deceased.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
    public async Task<IActionResult> Deceased(
        [FromRoute] string trn,
        [FromBody] SetDeceasedRequest request,
        [FromServices] SetDeceasedHandler handler)
    {
        var command = new SetDeceasedCommand(trn, request.DateOfDeath);
        await handler.Handle(command);
        return NoContent();
    }
}
