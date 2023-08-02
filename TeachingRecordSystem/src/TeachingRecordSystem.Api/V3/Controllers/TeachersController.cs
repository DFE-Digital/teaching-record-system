using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Controllers;

[ApiController]
[Route("teachers")]
[Authorize(AuthorizationPolicies.ApiKey)]
[SupportsReadOnlyMode]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{trn}")]
    [OpenApiOperation(
        operationId: "GetTeacherByTrn",
        summary: "Get teacher details by TRN",
        description: "Gets the details of the teacher corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), Description("The additional properties to include in the response.")] GetTeacherRequestIncludes? include)
    {
        var request = new GetTeacherRequest()
        {
            Trn = trn,
            Include = include ?? GetTeacherRequestIncludes.None,
            AccessMode = AccessMode.ApiKey
        };

        var response = await _mediator.Send(request);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPost("name-changes")]
    [OpenApiOperation(
        operationId: "CreateNameChange",
        summary: "Create name change request",
        description: "Creates a name change request for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNameChange([FromBody] CreateNameChangeRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }

    [HttpPost("date-of-birth-changes")]
    [OpenApiOperation(
        operationId: "CreateDobChange",
        summary: "Create DOB change request",
        description: "Creates a date of birth change request for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDateOfBirthChange([FromBody] CreateDateOfBirthChangeRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
