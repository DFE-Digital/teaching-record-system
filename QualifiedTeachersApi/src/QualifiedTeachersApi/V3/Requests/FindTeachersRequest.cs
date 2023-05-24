using FastEndpoints;
using MediatR;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record FindTeachersRequest : IRequest<FindTeachersResponse>
{
    [QueryParam, BindFrom("findBy")]
    public FindTeachersFindBy FindBy { get; init; }
    [QueryParam, BindFrom("lastName")]
    public string? LastName { get; init; }
    [QueryParam, BindFrom("dateOfBirth")]
    public DateOnly? DateOfBirth { get; init; }
}

public enum FindTeachersFindBy
{
    LastNameAndDateOfBirth = 1
}
