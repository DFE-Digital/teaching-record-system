namespace TeachingRecordSystem.Core.Models.SupportTasks;

public record OneLoginUserIdVerificationData : ISupportTaskData
{
    public required string OneLoginUserSubject { get; init; }
    public required string StatedFirstName { get; init; }
    public required string StatedLastName { get; init; }
    public required DateOnly StatedDateOfBirth { get; init; }
    public required string? StatedNationalInsuranceNumber { get; init; }
    public required string StatedTrn { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? TrnTokenTrn { get; init; }
    public required Guid ClientApplicationUserId { get; init; }
    public bool? Verified { get; init; }
    public Guid? PersonId { get; init; }
}
