using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20260416.Dtos;

namespace TeachingRecordSystem.Api.V3.V20260416.Controllers;

[Route("trn-request")]
public class TrnRequestController(ICommandDispatcher commandDispatcher, IMapper mapper, ICurrentUserProvider currentUserProvider) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetTrnRequest",
        Summary = "Get the authenticated person's TRN request",
        Description = "Gets the TRN request for the authenticated person.")]
    [Authorize(AuthorizationPolicies.TeacherAuthAccessToken)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync()
    {
        if (!currentUserProvider.TryGetTrnRequestId(out var trnRequestId))
        {
            return BadRequest();
        }

        var command = new GetTrnRequestCommand(
            trnRequestId,
            GetTrnRequestCommandOptions.SupportsDormantRequests | GetTrnRequestCommandOptions.SupportsRejectedRequests);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<TrnRequestInfo>(r)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }

    [HttpPut("activate")]
    [SwaggerOperation(
        OperationId = "ActivateTrnRequest",
        Summary = "Activate dormant TRN request",
        Description = "Activates the dormant request created by Teacher Auth.")]
    [Authorize(AuthorizationPolicies.TeacherAuthAccessToken)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateAsync()
    {
        if (!currentUserProvider.TryGetTrnRequestId(out var trnRequestId))
        {
            return BadRequest();
        }

        var command = new ActivateTrnRequestCommand(trnRequestId);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(
                r => StatusCode(
                    StatusCodes.Status200OK,
                    mapper.Map<TrnRequestInfo>(r.TrnRequestInfo)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }
}
