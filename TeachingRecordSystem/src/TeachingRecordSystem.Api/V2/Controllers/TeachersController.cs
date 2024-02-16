using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("teachers")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("find")]
    [SwaggerOperation(
        OperationId = "FindTeachers",
        Summary = "Find teachers",
        Description = "Returns teachers matching the specified criteria")]
    [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> FindTeachers(FindTeachersRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetTeacher",
        Summary = "Get teacher",
        Description = "Gets an individual teacher by their TRN")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> GetTeacher([FromRoute] GetTeacherRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }

    [HttpPatch("update/{trn}")]
    [SwaggerOperation(
        OperationId = "UpdateTeacher",
        Summary = "Update teacher",
        Description = "Updates a teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
    [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
    [RedactQueryParam("birthdate")]
    [Authorize(Policy = AuthorizationPolicies.UpdatePerson)]
    public async Task<IActionResult> Update([FromBody] UpdateTeacherRequest request)
    {
        await _mediator.Send(request);
        return NoContent();
    }
}
