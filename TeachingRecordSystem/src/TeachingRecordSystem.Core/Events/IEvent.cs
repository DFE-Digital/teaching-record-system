namespace TeachingRecordSystem.Core.Events;

public interface IEvent
{
    Guid EventId { get; }
    EventModels.RaisedByUserInfo RaisedBy { get; }
}
