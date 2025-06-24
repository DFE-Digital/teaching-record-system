using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Requests;
using TeachingRecordSystem.Api.V3.V20240606.Responses;

namespace TeachingRecordSystem.Api.V3.V20240606.Controllers;

[Route("person")]
public class PersonController(IMapper mapper) : ControllerBase
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
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetPersonRequestIncludes? include,
        [FromServices] GetPersonHandler handler)
    {
        var command = new GetPersonCommand(
            Trn: User.FindFirstValue("trn")!,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            DateOfBirth: null,
            NationalInsuranceNumber: null,
            new GetPersonCommandOptions()
            {
                ApplyLegacyAlertsBehavior = true
            });

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<GetPersonResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status403Forbidden);
    }

    [HttpPost("name-changes")]
    [SwaggerOperation(
        OperationId = "CreateNameChange",
        Summary = "Create name change request",
        Description = "Creates a name change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateNameChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateNameChangeAsync(
        [FromBody] CreateNameChangeRequestRequest request,
        [FromServices] CreateNameChangeRequestHandler handler)
    {
        var command = new CreateNameChangeRequestCommand()
        {
            Trn = User.FindFirstValue("trn")!,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = request.EmailAddress
        };

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<CreateNameChangeResponse>(r)));
    }

    [HttpPost("date-of-birth-changes")]
    [SwaggerOperation(
        OperationId = "CreateDobChange",
        Summary = "Create DOB change request",
        Description = "Creates a date of birth change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateDateOfBirthChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateDateOfBirthChangeAsync(
        [FromBody] CreateDateOfBirthChangeRequestRequest request,
        [FromServices] CreateDateOfBirthChangeRequestHandler handler)
    {
        var command = new CreateDateOfBirthChangeRequestCommand()
        {
            Trn = User.FindFirstValue("trn")!,
            DateOfBirth = request.DateOfBirth,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = request.EmailAddress
        };

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<CreateDateOfBirthChangeResponse>(r)));
    }
}
