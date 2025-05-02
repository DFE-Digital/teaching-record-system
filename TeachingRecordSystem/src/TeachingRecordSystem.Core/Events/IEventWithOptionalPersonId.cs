using Optional;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithOptionalPersonId
{
    Option<Guid> PersonId { get; }
}
