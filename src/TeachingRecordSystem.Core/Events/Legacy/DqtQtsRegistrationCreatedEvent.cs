using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record DqtQtsRegistrationCreatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required DqtQtsRegistration? QtsRegistration { get; init; }
}
