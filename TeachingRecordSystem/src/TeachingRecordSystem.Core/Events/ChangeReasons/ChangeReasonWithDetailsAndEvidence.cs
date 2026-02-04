namespace TeachingRecordSystem.Core.Events.ChangeReasons;

public record ChangeReasonWithDetailsAndEvidence : IChangeReason
{
    public required string Reason { get; init; }
    public required string? Details { get; init; }
    public required EventModels.File? EvidenceFile { get; init; }
}
