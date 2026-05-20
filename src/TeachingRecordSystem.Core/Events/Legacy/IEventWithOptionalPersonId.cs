using Optional;

namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithOptionalPersonId
{
    Option<Guid> PersonId { get; }
}
