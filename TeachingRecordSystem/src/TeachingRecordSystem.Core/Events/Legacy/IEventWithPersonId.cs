namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithPersonId
{
    Guid PersonId { get; }
}
