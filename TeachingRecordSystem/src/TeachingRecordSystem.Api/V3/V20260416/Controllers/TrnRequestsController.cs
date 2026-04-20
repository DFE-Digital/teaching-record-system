using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

namespace TeachingRecordSystem.Api.V3.V20260416.Controllers;

[Route("trn-requests")]
public class TrnRequestsController(ICommandDispatcher commandDispatcher, IMapper mapper, ICurrentUserProvider currentUserProvider) : ControllerBase
{
    [HttpPut("active/{requestId}")]
    [SwaggerOperation(
        OperationId = "ActivateTrnRequest",
        Summary = "Activate dormant TRN request",
        Description = "Activates a dormant request created by Teacher Auth.")]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync([FromRoute(Name = "requestId")] string trnRequestId)
    {
        if (!currentUserProvider.TryGetTrnRequestId(out var tokenTrnRequestId) || tokenTrnRequestId != trnRequestId)
        {
            return Forbid();
        }

        var command = new ActivateTrnRequestCommand(trnRequestId);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(
                r => StatusCode(
                    r.WasActivated ? StatusCodes.Status200OK : StatusCodes.Status204NoContent,
                    mapper.Map<TrnRequestInfo>(r.TrnRequestInfo)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }
}
