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
    [Route("itt-providers")]
    public class IttProvidersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IttProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        [SwaggerOperation(summary: "Gets a list of all ITT Providers")]
        [ProducesResponseType(typeof(GetIttProvidersResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIttProviders()
        {
            var request = new GetIttProvidersRequest();
            var response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
