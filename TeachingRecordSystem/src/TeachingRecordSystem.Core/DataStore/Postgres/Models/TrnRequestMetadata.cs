namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRequestMetadata
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required string[] Name { get; init; }
    public required DateOnly DateOfBirth { get; init; }
}
