using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.V20240606.Controllers;

[Route("teacher")]
public class TeacherController : ControllerBase
{
    [HttpGet("")]
    [RemovesFromApi]
    public IActionResult Get() => throw null!;

    [HttpPost("name-changes")]
    [RemovesFromApi]
    public IActionResult CreateNameChange() => throw null!;

    [HttpPost("date-of-birth-changes")]
    [RemovesFromApi]
    public IActionResult CreateDateOfBirthChange() => throw null!;
}
