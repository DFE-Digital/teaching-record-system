namespace TeachingRecordSystem.Core.ApiSchema.V3;

public record EventMapperContext
{
    // The application user that owns the webhook endpoint the message is being created for
    public required Guid ApplicationUserId { get; init; }
}
