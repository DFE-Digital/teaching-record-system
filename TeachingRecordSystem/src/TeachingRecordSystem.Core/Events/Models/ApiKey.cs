namespace TeachingRecordSystem.Core.Events.Models;

public record ApiKey
{
    public required Guid ApiKeyId { get; init; }
    public required Guid ApplicationUserId { get; init; }
    public required string Key { get; init; }
    public required DateTime? Expires { get; init; }

    public static ApiKey FromModel(Core.DataStore.Postgres.Models.ApiKey model) => new()
    {
        ApiKeyId = model.ApiKeyId,
        ApplicationUserId = model.ApplicationUserId,
        Key = model.Key,
        Expires = model.Expires
    };
}
