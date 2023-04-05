using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.ModelBinding;
using QualifiedTeachersApi.Security;
using QualifiedTeachersApi.V3.Requests;
using QualifiedTeachersApi.V3.Responses;
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

    [Authorize(AuthorizationPolicies.ApiKey)]
    [HttpGet("{trn}")]
    [SwaggerOperation(
        summary: "Get teacher details by TRN",
        description: "Gets the details of the teacher corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetTeacherRequestIncludes? include)
    {
        var request = new GetTeacherRequest()
        {
            Trn = trn,
            Include = include ?? GetTeacherRequestIncludes.None
        };

        var response = await _mediator.Send(request);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPost("name-changes")]
    [SwaggerOperation(summary: "Creates a name change for the teacher with the given TRN")]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNameChange([FromBody] CreateNameChangeRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
