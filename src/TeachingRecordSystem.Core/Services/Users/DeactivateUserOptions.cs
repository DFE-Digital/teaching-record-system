namespace TeachingRecordSystem.Core.Services.Users;

public record DeactivateUserOptions
{
    public required Guid UserId { get; init; }
    public required string? DeactivatedReason { get; init; }
    public required string? DeactivatedReasonDetail { get; init; }
    public required Guid? EvidenceFileId { get; init; }
}
