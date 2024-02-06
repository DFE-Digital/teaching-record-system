using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.V20240101.Requests;

public record FindTeachersRequest
{
    [FromQuery(Name = "findBy")]
    public FindTeachersFindBy FindBy { get; init; }
    [FromQuery(Name = "lastName")]
    public string? LastName { get; init; }
    [FromQuery(Name = "dateOfBirth")]
    public DateOnly? DateOfBirth { get; init; }
}

public enum FindTeachersFindBy
{
    LastNameAndDateOfBirth = 1
}
