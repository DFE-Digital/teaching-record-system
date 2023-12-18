using System.Text.Json;
using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public abstract record EventBase
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new();

    public required Guid EventId { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required RaisedByUserInfo RaisedBy { get; init; }

    public string GetEventName() => GetType().Name;
}
