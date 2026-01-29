using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250627.Requests;
using TeachingRecordSystem.Api.V3.V20250627.Responses;

namespace TeachingRecordSystem.Api.V3.V20250627.Controllers;

[Route("person")]
public class PersonController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder))] GetPersonRequestIncludes? include)
    {
        var command = new GetPersonCommand(
            Trn: User.FindFirstValue("trn")!,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            DateOfBirth: null,
            NationalInsuranceNumber: null);

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<GetPersonResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status403Forbidden);
    }
}
