using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240606.Controllers;

[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.CreateTrn)]
public class TrnRequestsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
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
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Gender = null
        };
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<TrnRequestInfo>(r)))
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
        var command = new GetTrnRequestCommand(requestId);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<TrnRequestInfo>(r)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }
}
