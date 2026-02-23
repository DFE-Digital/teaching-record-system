namespace TeachingRecordSystem.Core.Events.ChangeReasons;

public record PersonDetailsChangeReasonInfo : ChangeReasonWithDetailsAndEvidence
{
    public required string? NameChangeReason { get; init; }
    public required EventModels.File? NameChangeEvidenceFile { get; init; }
}
