using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("trns")]
public class TrnsController : ControllerBase
{
    [HttpGet("{trn}")]
    [ActionName("GetTrn")]
    [SwaggerOperation(
        OperationId = "GetTrn",
        Summary = "Get a TRN",
        Description = "Checks if the specified TRN exists.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey)]
    public async Task<IActionResult> GetTrnAsync(
        [FromRoute] string trn,
        [FromServices] GetTrnHandler handler)
    {
        var result = await handler.HandleAsync(new GetTrnCommand(trn));

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound)
            .MapErrorCode(ApiError.ErrorCodes.RecordIsNotActive, StatusCodes.Status400BadRequest)
            .MapErrorCode(
                ApiError.ErrorCodes.RecordIsMerged,
                e => RedirectPermanentPreserveMethod(
                    Url.Action("GetTrn", new { trn = (string)e.Data[ApiError.DataKeys.MergedWithTrn] })!));
    }
}
