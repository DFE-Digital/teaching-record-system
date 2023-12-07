namespace TeachingRecordSystem.Core.Events;

public interface IEventWithPersonId
{
    Guid PersonId { get; }
}
