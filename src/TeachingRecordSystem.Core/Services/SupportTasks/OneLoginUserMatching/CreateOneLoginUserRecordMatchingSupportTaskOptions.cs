namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public record CreateOneLoginUserRecordMatchingSupportTaskOptions
{
    public required string OneLoginUserSubject { get; init; }
    public required string OneLoginUserEmailAddress { get; init; }
    public required string[][]? VerifiedNames { get; init; }
    public required DateOnly[]? VerifiedDatesOfBirth { get; init; }
    public required string? StatedNationalInsuranceNumber { get; init; }
    public required string? StatedTrn { get; init; }
    public required Guid ClientApplicationUserId { get; init; }
    public required string? TrnTokenTrn { get; init; }
    public required string? YearQtsReceived { get; init; }
    public required Guid? TrainingProviderId { get; init; }
    public required string? TrainingProviderName { get; init; }
    public required Guid? SubjectId { get; init; }
    public required string? SubjectName { get; init; }
    public string? TrnRequestId { get; init; }
}
