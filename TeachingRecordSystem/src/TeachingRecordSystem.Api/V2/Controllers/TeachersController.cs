using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Filters;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[ApiController]
[Route("teachers")]
public class TeachersController : Controller
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("find")]
    [OpenApiOperation(
        operationId: "FindTeachers",
        summary: "Find teachers",
        description: "Returns teachers matching the specified criteria")]
    [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
    [SupportsReadOnlyMode]
    public async Task<IActionResult> FindTeachers(FindTeachersRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{trn}")]
    [OpenApiOperation(
        operationId: "GetTeacher",
        summary: "Get teacher",
        description: "Gets an individual teacher by their TRN")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [SupportsReadOnlyMode]
    public async Task<IActionResult> GetTeacher([FromRoute] GetTeacherRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }

    [HttpPatch("update/{trn}")]
    [OpenApiOperation(
        operationId: "UpdateTeacher",
        summary: "Update teacher",
        description: "Updates a teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
    [RedactQueryParam("birthdate")]
    public async Task<IActionResult> Update([FromBody] UpdateTeacherRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
