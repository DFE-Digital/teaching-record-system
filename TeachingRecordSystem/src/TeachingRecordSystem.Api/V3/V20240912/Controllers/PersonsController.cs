using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240912.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240912.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [HttpPut("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "SetQtls",
        Summary = "Set QTLS status for a teacher",
        Description = "Sets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> PutQtlsAsync(
        [FromRoute] string trn,
        [FromBody] SetQtlsRequest request)
    {
        var command = new SetQtlsCommand(trn, request.QtsDate);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<QtlsResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [HttpGet("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "GetQtls",
        Summary = "Get QTLS status for a teacher",
        Description = "Gets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> GetQtlsAsync([FromRoute] string trn)
    {
        var command = new GetQtlsCommand(trn);
        var result = await commandDispatcher.DispatchAsync(command);
        return result.ToActionResult(r => Ok(mapper.Map<QtlsResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
