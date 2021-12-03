using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DqtApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using DqtApi.DAL;
using DqtApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DqtApi
{
    [ApiController]
    [Route("v1/teachers")]
    public class TeachersController : ControllerBase
    {
        private readonly IDataverseAdaptor _dataverseAdaptor;
        public TeachersController(IDataverseAdaptor dataverseAdaptor) {
            _dataverseAdaptor = dataverseAdaptor;
        }

        [HttpGet("{trn}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeacher(
            [FromRoute] string trn,
            [FromQuery(Name = "birthdate"), SwaggerParameter(Required = true)] string birthDate,  // TODO model binder for yyyy-mm-dd format
            [FromQuery] string nino)
        {
            // Validate TRN
            if (!Regex.IsMatch(trn, @"^\d{7}$"))
            {
                return Problem(title: "Invalid TRN", statusCode: 400);
            }

            // Validate birthDate
            if (string.IsNullOrEmpty(birthDate))
            {
                return NotFound();
            }

            if (!DateTime.TryParseExact(birthDate, "yyyy-MM-dd", provider: null, style: DateTimeStyles.None, out DateTime parsedBirthdate))
            {
                return Problem(title: "Invalid birthdate", statusCode: 400);
            }

            var request = new GetTeacherRequest
            {
                BirthDate = parsedBirthdate,
                TRN = trn,
                NationalInsuranceNumber = nino
            };

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
