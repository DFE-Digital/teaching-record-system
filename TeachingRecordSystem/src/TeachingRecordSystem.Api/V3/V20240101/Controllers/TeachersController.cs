using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
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
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> Get(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetTeacherRequestIncludes? include,
        [FromServices] GetPersonHandler handler)
    {
        var command = new GetPersonCommand(
            trn,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            DateOfBirth: null);

        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = mapper.Map<GetTeacherResponse>(result);
        return Ok(response);
    }

    [HttpPost("name-changes")]
    [SwaggerOperation(
        OperationId = "CreateNameChange",
        Summary = "Create name change request",
        Description = "Creates a name change request for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.UpdatePerson)]
    public async Task<IActionResult> CreateNameChange(
        [FromBody] CreateNameChangeRequestRequest request,
        [FromServices] CreateNameChangeRequestHandler handler)
    {
        var command = mapper.Map<CreateNameChangeRequestCommand>(request);
        await handler.Handle(command);
        return NoContent();
    }

    [HttpPost("date-of-birth-changes")]
    [SwaggerOperation(
        OperationId = "CreateDobChange",
        Summary = "Create DOB change request",
        Description = "Creates a date of birth change request for the teacher with the given TRN.")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.UpdatePerson)]
    public async Task<IActionResult> CreateDateOfBirthChange(
        [FromBody] CreateDateOfBirthChangeRequestRequest request,
        [FromServices] CreateDateOfBirthChangeRequestHandler handler)
    {
        var command = mapper.Map<CreateDateOfBirthChangeRequestCommand>(request);
        await handler.Handle(command);
        return NoContent();
    }

    [HttpGet("")]
    [SwaggerOperation(
        OperationId = "FindTeachers",
        Summary = "Find teachers",
        Description = "Finds teachers with a TRN matching the specified criteria.")]
    [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> FindTeachers(
        FindTeachersRequest request,
        [FromServices] FindTeachersHandler handler)
    {
        var command = new FindTeachersCommand(request.LastName!, request.DateOfBirth!.Value);
        var result = await handler.Handle(command);

        var response = new FindTeachersResponse()
        {
            Total = result.Total,
            Query = request,
            Results = result.Items.Select(mapper.Map<FindTeachersResponseResult>).AsReadOnly()
        };

        return Ok(response);
    }
}
