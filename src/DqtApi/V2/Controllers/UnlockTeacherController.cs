using System.Threading.Tasks;
using DqtApi.V2.Requests;
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
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlockTeacher([FromRoute] UnlockTeacherRequest request)
        {
            try
            {
                await _mediator.Send(request);
                return NoContent();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
