using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Security;
using QualifiedTeachersApi.V3.Requests;
using QualifiedTeachersApi.V3.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Controllers;

[ApiController]
[Route("teacher")]
public class TeacherController : Controller
{
    private readonly IMediator _mediator;

    public TeacherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]
    [Route("")]
    [SwaggerOperation(
        summary: "Get teacher details",
        description: "Gets the details of the currently authenticated teacher")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get()
    {
        var trn = User.FindFirstValue("trn");

        if (trn is null)
        {
            return MissingOrInvalidTrn();
        }

        var request = new GetTeacherRequest()
        {
            Trn = trn
        };

        var response = await _mediator.Send(request);

        if (response is null)
        {
            return MissingOrInvalidTrn();
        }

        return Ok(response);

        IActionResult MissingOrInvalidTrn() => BadRequest();
    }

    [Authorize(AuthorizationPolicies.ApiKey)]
    [HttpGet("{Trn}")]
    [SwaggerOperation(
        summary: "Get teacher details by TRN",
        description: "Gets the details of the teacher corresponding to the given TRN")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] GetTeacherRequest request)
    {
        var response = await _mediator.Send(request);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
