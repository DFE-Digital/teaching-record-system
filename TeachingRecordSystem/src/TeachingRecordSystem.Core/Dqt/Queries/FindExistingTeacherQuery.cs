namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindingExistingTeachersQuery(string FirstName, string? MiddleName, string LastName, DateOnly birthDate) : ICrmQuery<FindingExistingTeachersResult[]>;

public record FindingExistingTeachersResult
{
    public required Guid TeacherId { get; init; }
    public required string[] MatchedAttributes { get; init; }
    public required bool HasActiveSanctions { get; init; }
    public required bool HasQtsDate { get; init; }
    public required bool HasEytsDate { get; init; }
}
