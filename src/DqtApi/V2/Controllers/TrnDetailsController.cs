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
    [Route("trn-details")]
    public class TrnDetailsController : Controller
    {
        private readonly IMediator _mediator;

        public TrnDetailsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("trn-details-match")]
        [SwaggerOperation(summary: "Returns trn details given an email")]
        [ProducesResponseType(typeof(GetTrnDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrnDetailsMatch(GetTrnDetailsRequest request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
