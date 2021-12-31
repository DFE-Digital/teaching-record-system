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
    [Route("trn-requests")]
    public class TrnRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrnRequestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{requestId}")]
        [SwaggerOperation(summary: "Retrieves a TRN request")]
        [ProducesResponseType(typeof(TrnRequestInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTrnRequest([FromRoute] GetTrnRequest request)
        {
            var response = await _mediator.Send(request);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
