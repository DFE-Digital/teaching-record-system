using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[Route("certificates")]
[Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
public class CertificatesController : ControllerBase
{
    [HttpGet]
    [Route("qts")]
    [SwaggerOperation(
        OperationId = "GetQtsCertificate",
        Summary = "Get QTS Certificate",
        Description = "Returns a PDF of the QTS Certificate for the authenticated teacher.")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQtsAsync([FromServices] GetQtsCertificateHandler handler)
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var command = new GetQtsCertificateCommand(trn);
        var result = await handler.HandleAsync(command);

        if (result is null)
        {
            return NotFound();
        }

        return result.ToFileResult();
    }

    [HttpGet]
    [Route("eyts")]
    [SwaggerOperation(
        OperationId = "GetEytsCertificate",
        Summary = "Get EYTS Certificate",
        Description = "Returns a PDF of the EYTS Certificate for the authenticated teacher.")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEytsAsync([FromServices] GetEytsCertificateHandler handler)
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var command = new GetEytsCertificateCommand(trn);
        var result = await handler.HandleAsync(command);

        if (result is null)
        {
            return NotFound();
        }

        return result.ToFileResult();
    }

    [HttpGet]
    [Route("induction")]
    [SwaggerOperation(
        OperationId = "GetInductionCertificate",
        Summary = "Induction Certificate",
        Description = "Returns a PDF of the Induction Certificate for the authenticated teacher.")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInductionAsync([FromServices] GetInductionCertificateHandler handler)
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var command = new GetInductionCertificateCommand(trn);
        var result = await handler.HandleAsync(command);

        if (result is null)
        {
            return NotFound();
        }

        return result.ToFileResult();
    }

    [HttpGet]
    [Route("npq/{qualificationId}")]
    [SwaggerOperation(
        OperationId = "GetNpqCertificate",
        Summary = "NPQ Certificate",
        Description = "Returns a PDF of the NPQ Certificate associated with the provided qualification ID.")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNpqAsync(
        [FromRoute, SwaggerParameter("The ID of the qualification record associated with the certificate.")] Guid qualificationId,
        [FromServices] GetNpqCertificateHandler handler)
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var command = new GetNpqCertificateCommand(trn, qualificationId);
        var result = await handler.HandleAsync(command);

        if (result is null)
        {
            return NotFound();
        }

        return result.ToFileResult();
    }
}
