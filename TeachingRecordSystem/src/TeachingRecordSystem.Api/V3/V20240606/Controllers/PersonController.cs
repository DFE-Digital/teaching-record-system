using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Requests;
using TeachingRecordSystem.Api.V3.V20240606.Responses;

namespace TeachingRecordSystem.Api.V3.V20240606.Controllers;

[Route("person")]
public class PersonController(ICommandDispatcher commandDispatcher, IMapper mapper) : ControllerBase
{
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]
    [EndpointName("GetCurrentPerson"),
        EndpointSummary("Get the authenticated person's details"),
        EndpointDescription("Gets the details for the authenticated person.")]
    [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), Description("The additional properties to include in the response.")] GetPersonRequestIncludes? include)
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

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<GetPersonResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status403Forbidden);
    }

    [HttpPost("name-changes")]
    [EndpointName("CreateNameChange"),
        EndpointSummary("Create name change request"),
        EndpointDescription("Creates a name change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateNameChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateNameChangeAsync(
        [FromBody] CreateNameChangeRequestRequest request)
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

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<CreateNameChangeResponse>(r)));
    }

    [HttpPost("date-of-birth-changes")]
    [EndpointName("CreateDobChange"),
        EndpointSummary("Create DOB change request"),
        EndpointDescription("Creates a date of birth change request for the authenticated teacher.")]
    [ProducesResponseType(typeof(CreateDateOfBirthChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    public async Task<IActionResult> CreateDateOfBirthChangeAsync(
        [FromBody] CreateDateOfBirthChangeRequestRequest request)
    {
        var command = new CreateDateOfBirthChangeRequestCommand()
        {
            Trn = User.FindFirstValue("trn")!,
            DateOfBirth = request.DateOfBirth,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = request.EmailAddress
        };

        var result = await commandDispatcher.DispatchAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<CreateDateOfBirthChangeResponse>(r)));
    }
}
