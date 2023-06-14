using System.ComponentModel;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Infrastructure.Security;
using QualifiedTeachersApi.V3.Requests;

namespace QualifiedTeachersApi.V3.Controllers;

[ApiController]
[Route("certificates")]
[Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
[SupportsReadOnlyMode]
public class CertificatesController : Controller
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("qts")]
    [OpenApiOperation(
        operationId: "GetQtsCertificate",
        summary: "Get QTS Certificate",
        description: "Returns a PDF of the QTS Certificate for the authenticated teacher.")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        return new FileContentResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
    }

    [HttpGet]
    [Route("eyts")]
    [OpenApiOperation(
        operationId: "GetEytsCertificate",
        summary: "Get EYTS Certificate",
        description: "Returns a PDF of the EYTS Certificate for the authenticated teacher.")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        return new FileContentResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
    }

    [HttpGet]
    [Route("induction")]
    [OpenApiOperation(
        operationId: "GetInductionCertificate",
        summary: "Induction Certificate",
        description: "Returns a PDF of the Induction Certificate for the authenticated teacher.")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        return new FileContentResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
    }

    [HttpGet]
    [Route("npq/{qualificationId}")]
    [OpenApiOperation(
        operationId: "GetNpqCertificate",
        summary: "NPQ Certificate",
        description: "Returns a PDF of the NPQ Certificate associated with the provided qualification ID.")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNpq([FromRoute][Description("The ID of the qualification record associated with the certificate.")] Guid qualificationId)
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

        return new FileContentResult(response.FileContents, "application/pdf")
        {
            FileDownloadName = response.FileDownloadName
        };
    }
}
