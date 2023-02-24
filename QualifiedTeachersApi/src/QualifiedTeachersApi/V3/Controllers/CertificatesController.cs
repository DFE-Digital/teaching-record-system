using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Security;
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
    public async Task<IActionResult> Get()
    {
        var trn = User.FindFirstValue("trn");

        if (trn is null)
        {
            return MissingOrInvalidTrn();
        }

        var request = new GetQtsCertificateRequest()
        {
            Trn = trn
        };

        var response = await _mediator.Send(request);

        return response ?? MissingOrInvalidTrn();

        IActionResult MissingOrInvalidTrn() => BadRequest();
    }
}
