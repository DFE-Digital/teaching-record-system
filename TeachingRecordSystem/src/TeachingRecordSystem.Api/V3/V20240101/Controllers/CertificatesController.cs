using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[ApiController]
[Route("certificates")]
[Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
public class CertificatesController : Controller
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("qts")]
    [SwaggerOperation(
        OperationId = "GetQtsCertificate",
        Summary = "Get QTS Certificate",
        Description = "Returns a PDF of the QTS Certificate for the authenticated teacher.")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQts()
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var request = new GetQtsCertificateRequest()
        {
            Trn = trn
        };

        var response = await _mediator.Send(request);
        if (response is null)
        {
            return NotFound();
        }

        return new FileStreamResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
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
    public async Task<IActionResult> GetEyts()
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var request = new GetEytsCertificateRequest()
        {
            Trn = trn
        };

        var response = await _mediator.Send(request);
        if (response is null)
        {
            return NotFound();
        }

        return new FileStreamResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
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
    public async Task<IActionResult> GetInduction()
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var request = new GetInductionCertificateRequest()
        {
            Trn = trn
        };

        var response = await _mediator.Send(request);
        if (response is null)
        {
            return NotFound();
        }

        return new FileStreamResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
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
    public async Task<IActionResult> GetNpq(
        [FromRoute, SwaggerParameter("The ID of the qualification record associated with the certificate.")] Guid qualificationId)
    {
        var trn = User.FindFirstValue("trn");
        if (trn is null)
        {
            return NotFound();
        }

        var request = new GetNpqCertificateRequest()
        {
            QualificationId = qualificationId,
            Trn = trn
        };

        var response = await _mediator.Send(request);
        if (response is null)
        {
            return NotFound();
        }

        return new FileStreamResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
    }
}
