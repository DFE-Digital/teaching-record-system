using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240814.Requests;
using TeachingRecordSystem.Api.V3.V20240814.Responses;

namespace TeachingRecordSystem.Api.V3.V20240814.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
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
