using System.ComponentModel;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TeachingRecordSystem.Api.Filters;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Controllers;

[ApiController]
[Route("teacher")]
[SupportsReadOnlyMode]
public class TeacherController : Controller
{
    private readonly IMediator _mediator;

    public TeacherController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(AuthorizationPolicies.IdentityUserWithTrn)]
    [HttpGet]
    [OpenApiOperation(
        operationId: "GetCurrentTeacher",
        summary: "Get the current teacher's details",
        description: "Gets the details for the authenticated teacher.")]
    [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery, ModelBinder(typeof(FlagsEnumStringListModelBinder)), Description("The additional properties to include in the response.")] GetTeacherRequestIncludes? include)
    {
        var trn = User.FindFirstValue("trn");

        if (trn is null)
        {
            return MissingOrInvalidTrn();
        }

        var request = new GetTeacherRequest()
        {
            Trn = trn,
            Include = include ?? GetTeacherRequestIncludes.None,
            AccessMode = AccessMode.IdentityAccessToken
        };

        var response = await _mediator.Send(request);

        if (response is null)
        {
            return MissingOrInvalidTrn();
        }

        return Ok(response);

        IActionResult MissingOrInvalidTrn() => BadRequest();
    }
}
