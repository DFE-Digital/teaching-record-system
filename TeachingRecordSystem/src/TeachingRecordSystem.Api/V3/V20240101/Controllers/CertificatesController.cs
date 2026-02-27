using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[Route("certificates")]
[Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
public class CertificatesController : ControllerBase
{
    [HttpGet]
    [Route("qts")]
    [EndpointName("GetQtsCertificate"),
        EndpointSummary("Get QTS Certificate"),
        EndpointDescription("Returns a PDF of the QTS Certificate for the authenticated teacher.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetQts() => StatusCode(StatusCodes.Status410Gone);

    [HttpGet]
    [Route("eyts")]
    [EndpointName("GetEytsCertificate"),
        EndpointSummary("Get EYTS Certificate"),
        EndpointDescription("Returns a PDF of the EYTS Certificate for the authenticated teacher.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetEyts() => StatusCode(StatusCodes.Status410Gone);

    [HttpGet]
    [Route("induction")]
    [EndpointName("GetInductionCertificate"),
        EndpointSummary("Induction Certificate"),
        EndpointDescription("Returns a PDF of the Induction Certificate for the authenticated teacher.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetInduction() => StatusCode(StatusCodes.Status410Gone);

    [HttpGet]
    [Route("npq/{qualificationId}")]
    [EndpointName("GetNpqCertificate"),
        EndpointSummary("NPQ Certificate"),
        EndpointDescription("Returns a PDF of the NPQ Certificate associated with the provided qualification ID.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetNpq() => StatusCode(StatusCodes.Status410Gone);
}
