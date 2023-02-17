using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.Filters;
using QualifiedTeachersApi.Logging;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Controllers
{
    [ApiController]
    [Route("teachers")]
    public class TeachersController : Controller
    {
        private readonly IMediator _mediator;

        public TeachersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("find")]
        [SwaggerOperation(summary: "Returns teachers matching the specified criteria")]
        [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindTeachers(FindTeachersRequest request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("{trn}")]
        [SwaggerOperation(
            summary: "Teacher",
            description: "Get an individual teacher by their TRN")]
        [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeacher([FromRoute] GetTeacherRequest request)
        {
            var response = await _mediator.Send(request);
            return response != null ? Ok(response) : NotFound();
        }

        [HttpPatch("update/{trn}")]
        [SwaggerOperation(summary: "Updates a Teacher record")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
        [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
        [RedactQueryParam("birthdate")]
        public async Task<IActionResult> Update([FromBody] UpdateTeacherRequest request)
        {
            await _mediator.Send(request);
            return NoContent();
        }
    }
}
