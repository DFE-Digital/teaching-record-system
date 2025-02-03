using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.V3.V20250203.Controllers;

[Route("certificates")]
[Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
public class CertificatesController : ControllerBase
{
    [HttpGet]
    [Route("qts")]
    [RemovesFromApi]
    public IActionResult GetQts() => throw null!;

    [HttpGet]
    [Route("eyts")]
    [RemovesFromApi]
    public IActionResult GetEyts() => throw null!;

    [HttpGet]
    [Route("induction")]
    [RemovesFromApi]
    public IActionResult GetInduction() => throw null!;

    [HttpGet]
    [Route("npq/{qualificationId}")]
    [RemovesFromApi]
    public IActionResult GetNpq() => throw null!;
}
