namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public record CreateOneLoginUserIdVerificationSupportTaskOptions
{
    public required string OneLoginUserSubject { get; init; }
    public required string OneLoginUserEmailAddress { get; init; }
    public required string? StatedNationalInsuranceNumber { get; init; }
    public required string? StatedTrn { get; init; }
    public required Guid ClientApplicationUserId { get; init; }
    public required string? TrnTokenTrn { get; init; }
    public required string StatedFirstName { get; init; }
    public required string StatedLastName { get; init; }
    public required DateOnly StatedDateOfBirth { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? YearQtsReceived { get; init; }
    public required Guid? TrainingProviderId { get; init; }
    public required string? TrainingProviderName { get; init; }
    public required Guid? SubjectId { get; init; }
    public required string? SubjectName { get; init; }
}
