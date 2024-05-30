namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindPotentialDuplicateContactsQuery : ICrmQuery<FindPotentialDuplicateContactsResult[]>
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
}

public record FindPotentialDuplicateContactsResult
{
    public required Guid TeacherId { get; init; }
    public required string[] MatchedAttributes { get; init; }
    public required bool HasActiveSanctions { get; init; }
    public required bool HasQtsDate { get; init; }
    public required bool HasEytsDate { get; init; }
}
