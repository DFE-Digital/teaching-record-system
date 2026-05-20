using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.V20240606.Controllers;

[Route("teachers")]
public class TeachersController : ControllerBase
{
    [HttpGet("{trn}")]
    [RemovesFromApi]
    public IActionResult GetTeacherByTrn() => throw null!;

    [HttpGet("")]
    [RemovesFromApi]
    public IActionResult FindTeacher() => throw null!;
}
