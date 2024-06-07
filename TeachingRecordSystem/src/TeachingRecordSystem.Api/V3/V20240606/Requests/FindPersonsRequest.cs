using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V3.V20240606.Requests;

public record FindPersonsRequest
{
    [FromQuery(Name = "findBy")]
    public FindPersonsFindBy FindBy { get; init; }
    [FromQuery(Name = "lastName")]
    public string? LastName { get; init; }
    [FromQuery(Name = "dateOfBirth")]
    public DateOnly? DateOfBirth { get; init; }
}

public enum FindPersonsFindBy
{
    LastNameAndDateOfBirth = 1
}
