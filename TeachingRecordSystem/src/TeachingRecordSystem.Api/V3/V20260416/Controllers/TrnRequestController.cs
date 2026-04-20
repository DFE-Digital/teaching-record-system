using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

namespace TeachingRecordSystem.Api.V3.V20260416.Controllers;

[Route("trn-request")]
public class TrnRequestController(ICommandDispatcher commandDispatcher, V20250425.ApiMapper mapper, ICurrentUserProvider currentUserProvider) : ControllerBase
{
    [HttpPut("activate")]
    [SwaggerOperation(
        OperationId = "ActivateTrnRequest",
        Summary = "Activate dormant TRN request",
        Description = "Activates the dormant request created by Teacher Auth.")]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status204NoContent)]
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
                    r.WasActivated ? StatusCodes.Status200OK : StatusCodes.Status204NoContent,
                    mapper.MapTrnRequestInfo(r.TrnRequestInfo)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }
}
