using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240307.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240307.Controllers;

[Route("trn-requests")]
[Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.CreateTrn)]
public class TrnRequestsController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [HttpPost("")]    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTrnRequestAsync(
        [FromBody] CreateTrnRequestRequest request)
    {
        var command = new CreateTrnRequestCommand()
        {
            RequestId = request.RequestId,
            FirstName = request.Person.FirstName,
            MiddleName = request.Person.MiddleName,
            LastName = request.Person.LastName,
            DateOfBirth = request.Person.DateOfBirth,
            EmailAddresses = request.Person.Email is string emailAddress ? [emailAddress] : [],
            NationalInsuranceNumber = request.Person.NationalInsuranceNumber,
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Gender = null
        };

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<TrnRequestInfo>(r)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestAlreadyCreated, StatusCodes.Status409Conflict);
    }

    [HttpGet("")]    [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrnRequestAsync([FromQuery] string requestId)
    {
        var command = new GetTrnRequestCommand(requestId);
        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<TrnRequestInfo>(r)))
            .MapErrorCode(ApiError.ErrorCodes.TrnRequestDoesNotExist, StatusCodes.Status404NotFound);
    }
}
