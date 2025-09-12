using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250327.Requests;
using TeachingRecordSystem.Api.V3.V20250327.Responses;

namespace TeachingRecordSystem.Api.V3.V20250327.Controllers;

[Route("person")]
public class PersonController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetCurrentPerson",
        Summary = "Get the authenticated person's details",
        Description = "Gets the details for the authenticated person.")]
    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetPersonRequestIncludes? include)
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
