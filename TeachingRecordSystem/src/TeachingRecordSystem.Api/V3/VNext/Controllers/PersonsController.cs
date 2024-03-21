using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Controllers;

[ApiController]
[Route("persons")]
[Authorize(Roles = ApiRoles.UpdatePerson)]
public class PersonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonsController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPut("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "PutQTLS",
        Summary = "Sets QTLS status for a teacher",
        Description = "Sets QTLS status for a teacher.")]
    [ProducesResponseType(typeof(QTLSInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(
        [FromBody] SetQTLSRequest request,
        [FromServices] SetQTLSHandler handler)
    {
        var command = new SetQTLSCommand(request.Trn!, request.AwardedDate);
        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = result.Adapt<QTLSInfo>();
        return Ok(response);
    }

    [HttpGet("{trn}/qtls")]
    [SwaggerOperation(
        OperationId = "GetQTLS",
        Summary = "Gets QTLS status for a teacher",
        Description = "Gets QTLS status for a teacher.")]
    [ProducesResponseType(typeof(QTLSInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
    [FromRoute] string trn,
    [FromServices] GetQTLSHandler handler)
    {
        var command = new GetQTLSCommand(trn);
        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = result.Adapt<QTLSInfo>();
        return Ok(response);
    }
}
