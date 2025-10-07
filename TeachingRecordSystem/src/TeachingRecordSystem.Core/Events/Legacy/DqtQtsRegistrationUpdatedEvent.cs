
using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record DqtQtsRegistrationUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required DqtQtsRegistration? QtsRegistration { get; init; }
    public required DqtQtsRegistration? OldQtsRegistration { get; init; }
    public required DqtQtsRegistrationUpdatedEventChanges Changes { get; init; }
}

public enum DqtQtsRegistrationUpdatedEventChanges
{
    None = 0,
    TeacherStatusValue = 1 << 0,
    EarlyYearsStatusValue = 1 << 1,
    QtsDate = 1 << 2,
    EytsDate = 1 << 3
}
