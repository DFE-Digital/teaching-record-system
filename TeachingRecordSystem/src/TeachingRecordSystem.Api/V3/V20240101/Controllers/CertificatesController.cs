using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[Route("certificates")]
[Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
public class CertificatesController : ControllerBase
{
    [HttpGet]
    [Route("qts")]    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetQts() => StatusCode(StatusCodes.Status410Gone);

    [HttpGet]
    [Route("eyts")]    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetEyts() => StatusCode(StatusCodes.Status410Gone);

    [HttpGet]
    [Route("induction")]    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetInduction() => StatusCode(StatusCodes.Status410Gone);

    [HttpGet]
    [Route("npq/{qualificationId}")]    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    public IActionResult GetNpq() => StatusCode(StatusCodes.Status410Gone);
}
