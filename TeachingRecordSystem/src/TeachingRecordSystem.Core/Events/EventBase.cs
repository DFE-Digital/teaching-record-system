using System.Text.Json;

namespace TeachingRecordSystem.Core.Events;

public abstract record EventBase
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new();

    public required DateTime CreatedUtc { get; init; }
    public required Guid SourceUserId { get; init; }
}
