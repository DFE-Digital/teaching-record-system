namespace TeachingRecordSystem.Core.Models.SupportTasks;

public record OneLoginUserRecordMatchingData : IOneLoginUserMatchingData
{
    public required string OneLoginUserSubject { get; init; }
    public required string OneLoginUserEmail { get; init; }
    public required string[][]? VerifiedNames { get; init; }
    public required DateOnly[]? VerifiedDatesOfBirth { get; init; }
    public required string? StatedNationalInsuranceNumber { get; init; }
    public required string? StatedTrn { get; init; }
    public required string? TrnTokenTrn { get; init; }
    public required Guid ClientApplicationUserId { get; init; }
    public Guid? PersonId { get; init; }
    public OneLoginUserRecordMatchingOutcome Outcome { get; init; }
    public OneLoginUserNotConnectingReason? NotConnectingReason { get; init; }
    public string? NotConnectingAdditionalDetails { get; init; }
    public string[][]? VerifiedOrStatedNames => VerifiedNames;
    public DateOnly[]? VerifiedOrStatedDatesOfBirth => VerifiedDatesOfBirth;
}

public enum OneLoginUserRecordMatchingOutcome
{
    NotMatched = 0,
    NotConnecting = 1,
    NoMatches = 2,
    Connected = 3
}
