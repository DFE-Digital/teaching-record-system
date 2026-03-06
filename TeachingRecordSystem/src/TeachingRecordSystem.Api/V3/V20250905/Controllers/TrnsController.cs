using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.V20250905.Controllers;

[Route("trns")]
public class TrnsController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpGet("{trn}")]
    [ActionName("GetTrn")]
    [EndpointName("GetTrn"),
        EndpointSummary("Get a TRN"),
        EndpointDescription("Checks if the specified TRN exists.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey)]
    public async Task<IActionResult> GetTrnAsync(
        [FromRoute] string trn)
    {
        var result = await commandDispatcher.DispatchAsync(new GetTrnCommand(trn));

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.RecordIsDeactivated, StatusCodes.Status410Gone)
            .MapErrorCode(
                ApiError.ErrorCodes.RecordIsMerged,
                e => RedirectPermanentPreserveMethod(
                    Url.Action("GetTrn", new { trn = (string)e.Data[ApiError.DataKeys.MergedWithTrn] })!));
    }
}
