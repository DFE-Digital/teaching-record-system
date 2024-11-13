namespace TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

public record TrnRequestMetadataMessage
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string VerifiedOneLoginUserSubject { get; init; }
}
