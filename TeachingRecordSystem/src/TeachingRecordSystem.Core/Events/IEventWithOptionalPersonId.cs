namespace TeachingRecordSystem.Core.Events;

public interface IEventWithOptionalPersonId
{
    Guid? PersonId { get; }
}
