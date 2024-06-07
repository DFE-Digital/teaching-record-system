using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.Operations;
using GetTeacherDtoVersion = TeachingRecordSystem.Api.V3.V20240101;

namespace TeachingRecordSystem.Api.V3.V20240416.Controllers;

[Route("teachers")]
public class TeachersController(IMapper mapper) : ControllerBase
{
    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetTeacherByTrn",
        Summary = "Get teacher details by TRN",
        Description = "Gets the details of the teacher corresponding to the given TRN.")]
    [ProducesResponseType(typeof(GetTeacherDtoVersion.Responses.GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [Authorize(Policy = AuthorizationPolicies.GetPerson)]
    public async Task<IActionResult> Get(
        [FromRoute] string trn,
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), SwaggerParameter("The additional properties to include in the response.")] GetTeacherDtoVersion.Requests.GetTeacherRequestIncludes? include,
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified dateOfBirth, if provided.")] DateOnly? dateOfBirth,
        [FromServices] GetPersonHandler handler)
    {
        var command = new GetPersonCommand(
            trn,
            include is not null ? (GetPersonCommandIncludes)include : GetPersonCommandIncludes.None,
            dateOfBirth);

        var result = await handler.Handle(command);

        if (result is null)
        {
            return NotFound();
        }

        var response = mapper.Map<GetTeacherDtoVersion.Responses.GetTeacherResponse>(result);
        return Ok(response);
    }
}
