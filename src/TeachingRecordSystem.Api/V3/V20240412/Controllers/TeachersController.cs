using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.V20240412.Controllers;

[Route("teachers")]
public class TeachersController : ControllerBase
{
    [HttpPost("name-changes")]
    [RemovesFromApi]
    public IActionResult CreateNameChange() => throw null!;

    [HttpPost("date-of-birth-changes")]
    [RemovesFromApi]
    public IActionResult CreateDateOfBirthChange() => throw null!;
}
