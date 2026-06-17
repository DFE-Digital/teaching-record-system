using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20250203;
using CreateTrnRequestRequest = TeachingRecordSystem.Api.V3.V20250425.Requests.CreateTrnRequestRequest;
using Gender = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.Gender;
using TrnRequestInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos.TrnRequestInfo;

namespace TeachingRecordSystem.Api.V3.V20260515.Controllers;

[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.CreateTrn)]
public class TrnRequestsController(ICommandDispatcher commandDispatcher) : ControllerBase
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
    public async Task<IActionResult> CreateTrnRequestAsync([FromBody] CreateTrnRequestRequest request)
    {
        var command = new CreateTrnRequestCommand()
        {
            RequestId = request.RequestId,
            FirstName = request.Person.FirstName,
            MiddleName = request.Person.MiddleName,
            LastName = request.Person.LastName,
            DateOfBirth = request.Person.DateOfBirth,
            EmailAddresses = request.Person.EmailAddresses ?? [],
            NationalInsuranceNumber = request.Person.NationalInsuranceNumber,
            IdentityVerified = request.IdentityVerified,
            OneLoginUserSubject = request.OneLoginUserSubject,
            Gender = request.Person.Gender is Gender gender ? Core.Models.Gender.Create(gender) : null
        };

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(TrnRequestInfo.Create(r)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestAlreadyCreated, StatusCodes.Status409Conflict);
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
    public async Task<IActionResult> GetTrnRequestAsync([FromQuery] string requestId)
    {
        var command = new GetTrnRequestCommand(
            requestId,
            GetTrnRequestCommandOptions.SupportsDormantRequests | GetTrnRequestCommandOptions.SupportsRejectedRequests);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(TrnRequestInfo.Create(r)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }
}
