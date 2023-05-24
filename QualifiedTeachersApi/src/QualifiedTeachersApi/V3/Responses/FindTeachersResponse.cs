using QualifiedTeachersApi.V3.Requests;

namespace QualifiedTeachersApi.V3.Responses;

public record FindTeachersResponse
{
    public required int Total { get; init; }
    public required FindTeachersRequest Query { get; init; }
    public required IReadOnlyCollection<FindTeachersResponseResult> Results { get; init; }
}

public record FindTeachersResponseResult
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
}
