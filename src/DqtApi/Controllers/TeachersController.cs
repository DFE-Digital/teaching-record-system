using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DqtApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using DqtApi.DAL;

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
            if (!DateTime.TryParseExact(birthDate, "yyyy-MM-dd", provider: null, style: DateTimeStyles.None, out _))
            {
                return Problem(title: "Invalid birthdate", statusCode: 400);
            }

            try
            {
                var teacher = await _dataverseAdaptor.GetTeacherByTRN(trn);

                if (teacher == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(new GetTeacherResponse(teacher));
                }

            }
            catch (MoreThanOneMatchingTeacherException)
            {
                return Problem(title: $"More than one teacher with TRN {trn}", statusCode: 400);
            }
        }
    }
}