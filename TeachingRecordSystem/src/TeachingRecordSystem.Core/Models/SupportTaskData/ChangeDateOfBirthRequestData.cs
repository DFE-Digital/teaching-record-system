namespace TeachingRecordSystem.Core.Models.SupportTaskData;

public record ChangeDateOfBirthRequestData
{
    public required DateOnly DateOfBirth { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
    public required SupportRequestOutcome? ChangeRequestOutcome { get; init; }
}
