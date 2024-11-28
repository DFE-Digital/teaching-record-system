using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240101.Requests;
using TeachingRecordSystem.Api.V3.V20240101.Responses;

namespace TeachingRecordSystem.Api.V3.V20240101.Controllers;

[Route("teachers")]
public class TeachersController(IMapper mapper) : ControllerBase
{
    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetTeacherByTrn",
        Summary = "Get teacher details by TRN",
        Description = "Gets the details of the teacher corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> GetAsync(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetTeacherRequestIncludes? include,
        [FromServices] GetPersonHandler handler)
    {
        var command = new GetPersonCommand(
            trn,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            DateOfBirth: null,
            ApplyLegacyAlertsBehavior: true);

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(r => Ok(mapper.Map<GetTeacherResponse>(r)))
            .MapErrorCode(ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [HttpPost("name-changes")]
    [SwaggerOperation(
        OperationId = "CreateNameChange",
        Summary = "Create name change request",
        Description = "Creates a name change request for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
    public async Task<IActionResult> CreateNameChangeAsync(
        [FromBody] CreateNameChangeRequestRequest request,
        [FromServices] CreateNameChangeRequestHandler handler)
    {
        var command = new CreateNameChangeRequestCommand()
        {
            Trn = request.Trn,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = null
        };

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent());
    }

    [HttpPost("date-of-birth-changes")]
    [SwaggerOperation(
        OperationId = "CreateDobChange",
        Summary = "Create DOB change request",
        Description = "Creates a date of birth change request for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.UpdatePerson)]
    public async Task<IActionResult> CreateDateOfBirthChangeAsync(
        [FromBody] CreateDateOfBirthChangeRequestRequest request,
        [FromServices] CreateDateOfBirthChangeRequestHandler handler)
    {
        var command = new CreateDateOfBirthChangeRequestCommand()
        {
            Trn = request.Trn,
            DateOfBirth = request.DateOfBirth,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileUrl = request.EvidenceFileUrl,
            EmailAddress = null
        };

        var result = await handler.HandleAsync(command);

        return result.ToActionResult(_ => NoContent());
    }

    [HttpGet("")]
    [SwaggerOperation(
        OperationId = "FindTeachers",
        Summary = "Find teachers",
        Description = "Finds teachers with a TRN matching the specified criteria.")]
    [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindTeachersAsync(
        FindTeachersRequest request,
        [FromServices] FindPersonByLastNameAndDateOfBirthHandler handler)
    {
        var command = new FindPersonByLastNameAndDateOfBirthCommand(request.LastName!, request.DateOfBirth!.Value);
        var result = await handler.HandleAsync(command);

        return result.ToActionResult(r =>
            Ok(new FindTeachersResponse()
            {
                Total = r.Total,
                Query = request,
                Results = r.Items.Select(mapper.Map<FindTeachersResponseResult>).AsReadOnly()
            }));
    }
}
