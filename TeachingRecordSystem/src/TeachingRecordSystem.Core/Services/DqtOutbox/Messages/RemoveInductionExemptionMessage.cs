namespace TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

public record RemoveInductionExemptionMessage
{
    public required Guid PersonId { get; init; }
    public required Guid ExemptionReasonId { get; init; }
    public Guid? TrsUserId { get; init; }
    public Guid? DqtUserId { get; init; }
    public string? DqtUserName { get; init; }
}
