namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRequestMetadata
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string? VerifiedOneLoginUserSubject { get; init; }
}
