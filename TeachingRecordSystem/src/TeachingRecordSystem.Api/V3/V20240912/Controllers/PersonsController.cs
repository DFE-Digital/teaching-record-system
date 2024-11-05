using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240912.ApiModels;
using TeachingRecordSystem.Api.V3.V20240912.Requests;

namespace TeachingRecordSystem.Api.V3.V20240912.Controllers;

[Route("persons")]
public class PersonsController(IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "SetQtls",
        Summary = "Set QTLS status for a teacher",
        Description = "Sets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsResponse), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(QtlsResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> GetQtls(
    [FromRoute] string trn,
        [FromServices] GetQtlsHandler handler)
    {
        var command = new GetQtlsCommand(trn);
        var result = await handler.Handle(command);
        var response = mapper.Map<QtlsResponse?>(result);
        return response is not null ? Ok(response) : NotFound();
    }
}
