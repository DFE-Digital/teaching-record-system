namespace TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

public record SetInductionRequiredToCompleteMessage
{
    public required Guid PersonId { get; init; }
    public Guid? TrsUserId { get; init; }
    public Guid? DqtUserId { get; init; }
    public string? DqtUserName { get; init; }
}
