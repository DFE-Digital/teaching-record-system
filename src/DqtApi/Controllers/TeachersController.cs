using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using DqtApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi
{
    [ApiController]
    [Route("v1/teachers")]
    public class TeachersController : ControllerBase
    {
        private readonly IDataverseAdaptor _dataverseAdaptor;

        public TeachersController(IDataverseAdaptor dataverseAdaptor)
        {
            _dataverseAdaptor = dataverseAdaptor;
        }

        [HttpGet("{trn}")]
        [SwaggerOperation(
            summary: "Teacher",
            description: "Get an individual teacher by their TRN")]
        [ProducesResponseType(typeof(GetTeacherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacher([FromRoute]GetTeacherRequest request)           
        {            
            if (!request.BirthDate.HasValue)
            {
                return NotFound();
            }

            var matchingTeachers = await _dataverseAdaptor.GetMatchingTeachersAsync(request);

            var teacher = request.SelectMatch(matchingTeachers);

            if (teacher == null)
            {
                return NotFound();
            }
            else
            {
                var qualifications = await _dataverseAdaptor.GetQualificationsAsync(teacher.Id);

                if (qualifications.Any())
                {
                    teacher.dfeta_contact_dfeta_qualification = qualifications;
                }

                return Ok(new GetTeacherResponse(teacher));
            }
        }
    }
}
