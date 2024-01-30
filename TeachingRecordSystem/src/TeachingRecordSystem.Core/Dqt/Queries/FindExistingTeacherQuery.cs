namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindExistingTrnQuery(string FirstName, string? MiddleName, string LastName, DateOnly birthDate) : ICrmQuery<FindExistingTrnResult?>;

public record FindExistingTrnResult
{
    public required Guid TeacherId { get; init; }
    public required string[] MatchedAttributes { get; init; }
    public required bool HasActiveSanctions { get; init; }
    public required bool HasQtsDate { get; init; }
    public required bool HasEytsDate { get; init; }
}
