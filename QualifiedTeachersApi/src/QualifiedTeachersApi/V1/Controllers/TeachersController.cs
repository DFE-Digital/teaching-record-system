using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Infrastructure.Logging;
using QualifiedTeachersApi.V1.Requests;
using QualifiedTeachersApi.V1.Responses;

namespace QualifiedTeachersApi.V1.Controllers;

[ApiController]
[Route("teachers")]
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
        operationId: "GetTeacher",
        summary: "Get teacher",
        description: "Gets a teacher by their DOB and either TRN or NINO")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [RedactQueryParam("birthdate"), RedactQueryParam("nino")]
    public async Task<IActionResult> GetTeacher([FromRoute] GetTeacherRequest request)
    {
        var response = await _mediator.Send(request);
        return response != null ? Ok(response) : NotFound();
    }
}
