using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optional.Unsafe;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[Route("persons")]
public class PersonsController : ControllerBase
{
    [HttpPut("{trn}/welsh-induction")]
    [SwaggerOperation(
        OperationId = "SetPersonWelshInductionStatus",
        Summary = "Set person induction status",
        Description = "Sets the induction details of the person with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.SetWelshInduction)]
    public async Task<IActionResult> SetWelshInductionStatusAsync(
        [FromRoute] string trn,
        [FromBody] SetWelshInductionStatusRequest request,
        [FromServices] SetWelshInductionStatusHandler handler)
    {
        var command = new SetWelshInductionStatusCommand(
            trn,
            request.Passed,
            request.StartDate,
            request.CompletedDate);

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent())
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }
}
