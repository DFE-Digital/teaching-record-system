using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;
using TeachingRecordSystem.Api.V3.V20240101.Requests;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[ApiController]
[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.CreateTrn)]
public class TrnRequestsController : ControllerBase
{
    [HttpPost("")]
    [SwaggerOperation(
        OperationId = "CreateTrnRequest",
        Summary = "Creates a TRN request",
        Description = """
        Creates a new TRN request using the personally identifiable information in the request body.
        If the request can be fulfilled immediately the response's status property will be 'Completed' and a TRN will also be returned.
        Otherwise, the response's status property will be 'Pending' and the GET endpoint should be polled until a 'Completed' status is returned.
        """)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [MapError(10029, statusCode: StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTrnRequest(
        [FromBody] CreateTrnRequestRequest request,
        [FromServices] CreateTrnRequestHandler handler)
    {
        var command = new CreateTrnRequestCommand()
        {
            RequestId = request.RequestId,
            FirstName = request.Person.FirstName,
            MiddleName = request.Person.MiddleName,
            LastName = request.Person.LastName,
            DateOfBirth = request.Person.DateOfBirth,
            Email = request.Person.Email,
            NationalInsuranceNumber = request.Person.NationalInsuranceNumber
        };
        var result = await handler.Handle(command);

        var response = result.Adapt<TrnRequestInfo>();
        return Ok(response);
    }

    [HttpGet("")]
    [SwaggerOperation(
        OperationId = "GetTrnRequest",
        Summary = "Get the TRN request's details",
        Description = """
        Gets the TRN request for the requestId specified in the query string.
        If the request's status is 'Completed' a TRN will also be returned.
        """)]
    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrnRequest(
        [FromQuery] string requestId,
        [FromServices] GetTrnRequestHandler handler)
    {
        var command = new GetTrnRequestCommand(requestId);
        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = result.Adapt<TrnRequestInfo>();
        return Ok(response);
    }
}
