using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240912.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240912.Controllers;

[Route("persons")]
public class PersonsController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPut("{trn}/qtls")]
    [OperationId("SetQtls"),
        EndpointSummary("Set QTLS status for a teacher"),
        EndpointDescription("Sets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> PutQtlsAsync(
        [FromRoute] string trn,
        [FromBody] SetQtlsRequest request)
    {
        var command = new SetQtlsCommand(trn, request.QtsDate);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(QtlsResponse.Create(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [HttpGet("{trn}/qtls")]
    [OperationId("GetQtls"),
        EndpointSummary("Get QTLS status for a teacher"),
        EndpointDescription("Gets the QTLS status for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(QtlsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.AssignQtls)]
    public async Task<IActionResult> GetQtlsAsync([FromRoute] string trn)
    {
        var command = new GetQtlsCommand(trn);
        var result = await commandDispatcher.DispatchAsync(command);
        return result.ToActionResult(r => Ok(QtlsResponse.Create(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
