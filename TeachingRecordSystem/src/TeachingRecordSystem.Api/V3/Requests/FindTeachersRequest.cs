using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record FindTeachersRequest : IRequest<FindTeachersResponse>
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
