using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Security;
using QualifiedTeachersApi.V3.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Controllers;

[ApiController]
[Route("teachers")]
[Authorize(AuthorizationPolicies.ApiKey)]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("name-changes")]
    [SwaggerOperation(summary: "Set NPQ qualification for a teacher")]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNameChange([FromBody] CreateNameChangeRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
