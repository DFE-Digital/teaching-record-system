using System.Linq;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.V1.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V1.Controllers
{
    [ApiController]
    [Route("teachers")]
    public class TeachersController : ControllerBase
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public TeachersController(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        [HttpGet("{trn}")]
        [SwaggerOperation(
            summary: "Teacher",
            description: "Get an individual teacher by their TRN")]
        [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacher([FromRoute] GetTeacherRequest request)           
        {            
            if (!request.BirthDate.HasValue)
            {
                return NotFound();
            }

            var matchingTeachers = await _dataverseAdapter.GetMatchingTeachersAsync(request);

            var teacher = request.SelectMatch(matchingTeachers);

            if (teacher == null)
            {
                return NotFound();
            }
            else
            {
                var qualifications = await _dataverseAdapter.GetQualificationsAsync(teacher.Id);

                if (qualifications.Any())
                {
                    teacher.dfeta_contact_dfeta_qualification = qualifications;
                }

                return Ok(new GetTeacherResponse(teacher));
            }
        }
    }
}
