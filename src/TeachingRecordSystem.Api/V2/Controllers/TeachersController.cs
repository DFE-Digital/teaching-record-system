using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V2.Handlers;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Controllers;

[Route("teachers")]
public class TeachersController : ControllerBase
{
    [HttpGet("find")]
    [SwaggerOperation(
        OperationId = "FindTeachers",
        Summary = "Find teachers",
        Description = "Returns teachers matching the specified criteria")]
    [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> FindTeachersAsync(
        FindTeachersRequest request,
        [FromServices] FindTeachersHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{trn}")]
    [SwaggerOperation(
        OperationId = "GetTeacher",
        Summary = "Get teacher",
        Description = "Gets an individual teacher by their TRN")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [Authorize(Policy = AuthorizationPolicies.ApiKey, Roles = ApiRoles.GetPerson)]
    public async Task<IActionResult> GetTeacherAsync(
        [FromRoute] GetTeacherRequest request,
        [FromServices] GetTeacherHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        return response is not null ? Ok(response) : NotFound();
    }
}
