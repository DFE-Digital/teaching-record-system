using System.Threading.Tasks;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Controllers
{
    [ApiController]
    [Route("unlock-teacher")]
    public class UnlockTeacherController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnlockTeacherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{teacherId}")]
        [SwaggerOperation(description: "Unlocks the teacher record allowing the teacher to sign in to the portals")]
        [ProducesResponseType(typeof(UnlockTeacherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockTeacher([FromRoute] UnlockTeacherRequest request)
        {
            try
            {
                return Ok(await _mediator.Send(request));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
