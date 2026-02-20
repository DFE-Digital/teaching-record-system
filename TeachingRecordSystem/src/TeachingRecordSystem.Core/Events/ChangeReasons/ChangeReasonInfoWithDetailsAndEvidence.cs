namespace TeachingRecordSystem.Core.Events.ChangeReasons;

public record ChangeReasonInfoWithDetailsAndEvidence : IChangeReasonInfo
{
    public required string? Reason { get; init; }
    public required string? Details { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
}
