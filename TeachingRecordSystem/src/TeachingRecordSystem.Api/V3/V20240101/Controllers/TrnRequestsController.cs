using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.ApiKey)]
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
    public IActionResult CreateTrnRequest([FromBody] CreateTrnRequestBody request) => throw new NotImplementedException();

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
    public IActionResult GetTrnRequest([FromQuery] string requestId) => throw new NotImplementedException();

    public record CreateTrnRequestBody
    {
        public required string RequestId { get; init; }
        public required TrnRequestPerson Person { get; init; }
    }

    public record TrnRequestPerson
    {
        public required string FirstName { get; init; }
        public string? MiddleName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly DateOfBirth { get; init; }
        public string? Email { get; init; }
        public string? NationalInsuranceNumber { get; init; }
    }

    public record TrnRequestInfo
    {
        public required string RequestId { get; init; }
        public required TrnRequestPerson Person { get; init; }
        public required TrnRequestStatus Status { get; init; }
        public string? Trn { get; init; }
    }

    public enum TrnRequestStatus
    {
        Pending = 0,
        Completed = 1
    }
}
