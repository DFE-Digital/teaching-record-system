using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240814.Requests;
using TeachingRecordSystem.Api.V3.V20240814.Responses;

namespace TeachingRecordSystem.Api.V3.V20240814.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
    [HttpPost("find")]
    [SwaggerOperation(
        OperationId = "FindPersons",
        Summary = "Find persons",
        Description = "Finds persons matching the specified criteria.")]
    [ProducesResponseType(typeof(FindPersonsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> FindTeachers(
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
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> FindTeachers(
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
}
