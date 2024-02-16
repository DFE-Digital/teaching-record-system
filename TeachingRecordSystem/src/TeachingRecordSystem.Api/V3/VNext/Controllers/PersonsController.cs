using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[ApiController]
[Route("persons")]
public class PersonsController : ControllerBase
{
    [HttpPut("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "PutQtls",
        Summary = "Sets QTLS status for a teacher",
        Description = "Sets QTLS status for a teacher.")]
    [ProducesResponseType(typeof(QtlsInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [MapError(10030, statusCode: StatusCodes.Status202Accepted)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.AssignQtls)]
    public async Task<IActionResult> Put(
        [FromBody] SetQtlsRequest request,
        [FromServices] SetQtlsHandler handler)
    {
        var command = new SetQtlsCommand(request.Trn!, request.QTSDate);
        var result = await handler.Handle(command);
        var response = result.Adapt<QtlsInfo>();
        return Ok(response);
    }

    [HttpGet("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "GetQtls",
        Summary = "Gets QTLS status for a teacher",
        Description = "Gets QTLS status for a teacher.")]
    [ProducesResponseType(typeof(QtlsInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.AssignQtls)]
    public async Task<IActionResult> Get(
        [FromRoute] string trn,
        [FromServices] GetQtlsHandler handler)
    {
        var command = new GetQtlsCommand(trn);
        var result = await handler.Handle(command);
        var response = result.Adapt<QtlsInfo>();
        return Ok(response);
    }
}