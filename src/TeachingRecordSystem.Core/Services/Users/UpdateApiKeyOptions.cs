using Optional;

namespace TeachingRecordSystem.Core.Services.Users;

public record UpdateApiKeyOptions
{
    public required Guid ApiKeyId { get; init; }
    public Option<DateTime?> Expires { get; init; }
}
