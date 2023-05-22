﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Controllers;

[ApiController]
[Route("unlock-teacher")]
[SupportsReadOnlyMode]
public class UnlockTeacherController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnlockTeacherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{teacherId}")]
    [OpenApiOperation(
        operationId: "UnlockTeacher",
        summary: "Unlock teacher",
        description: "Unlocks the teacher record allowing the teacher to sign in to the portals")]
    [ProducesResponseType(typeof(UnlockTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockTeacher([FromRoute] UnlockTeacherRequest request)
    {
        try
        {
            return Ok(await _mediator.Send(request));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
