using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.V20240416.Controllers;

[Route("teachers")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
        [FromQuery, SwaggerParameter("Adds an additional check that the record has the specified dateOfBirth, if provided.")] DateOnly? dateOfBirth)
    {
        var request = new GetTeacherRequest()
        {
            Trn = trn,
            Include = include ?? GetTeacherRequestIncludes.None,
            AccessMode = AccessMode.ApiKey
        };

        var response = await _mediator.Send(request);

        if (response is null || (dateOfBirth.HasValue && dateOfBirth.Value != response.DateOfBirth))
        {
            return NotFound();
        }

        return Ok(response);
    }
}
