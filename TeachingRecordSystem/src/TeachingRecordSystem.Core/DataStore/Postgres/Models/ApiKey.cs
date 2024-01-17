namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class ApiKey
{
    public const string KeyUniqueIndexName = "ix_api_keys_key";

    public const int KeyMaxLength = 100;
    public const int KeyMinLength = 16;

    public required Guid ApiKeyId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public required Guid ApplicationUserId { get; init; }
    public ApplicationUser ApplicationUser { get; } = null!;
    public required string Key { get; init; }
    public DateTime? Expires { get; set; }
}
