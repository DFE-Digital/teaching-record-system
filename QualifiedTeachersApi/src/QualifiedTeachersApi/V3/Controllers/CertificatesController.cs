using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Infrastructure.Security;
using QualifiedTeachersApi.V3.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Controllers;

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
        summary: "QTS Certificate",
        description: "Returns a PDF of the QTS Certificate for the provided TRN holder")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
    summary: "EYTS Certificate",
    description: "Returns a PDF of the EYTS Certificate for the provided TRN holder")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
    summary: "Induction Certificate",
    description: "Returns a PDF of the Induction Certificate for the provided TRN holder")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
        summary: "NPQ Certificate",
        description: "Returns a PDF of the NPQ Certificate associated with the provided Qualification ID")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNpq(GetNpqCertificateRequest request)
    {
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
