using System.Threading.Tasks;
using DqtApi.Filters;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Controllers
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

        [HttpPatch("update/{trn}")]
        [SwaggerOperation(summary: "Updates a Teacher record")]
        [ProducesResponseType(typeof(FindTeachersResponse), StatusCodes.Status204NoContent)]
        [MapError(10001, statusCode: StatusCodes.Status404NotFound)]
        [MapError(10002, statusCode: StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromBody] UpdateTeacherRequest request)
        {
            await _mediator.Send(request);
            return NoContent();
        }
    }
}
