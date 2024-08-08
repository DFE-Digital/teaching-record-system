using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.V20240606.Requests;

public record FindPersonRequest
{
    [FromQuery(Name = "findBy")]
    public FindPersonFindBy FindBy { get; init; }
    [FromQuery(Name = "lastName")]
    public string? LastName { get; init; }
    [FromQuery(Name = "dateOfBirth")]
    public DateOnly? DateOfBirth { get; init; }
}

public enum FindPersonFindBy
{
    LastNameAndDateOfBirth = 1
}
