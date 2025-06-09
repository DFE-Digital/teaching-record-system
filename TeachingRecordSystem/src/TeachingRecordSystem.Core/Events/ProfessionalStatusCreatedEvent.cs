using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record ProfessionalStatusCreatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required ProfessionalStatusCreatedEventChanges Changes { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
}

[Flags]
public enum ProfessionalStatusCreatedEventChanges
{
    None = 0,
    PersonQtsDate = 1 << 0,
    PersonEytsDate = 1 << 1,
    PersonHasEyps = 1 << 2,
    PersonPqtsDate = 1 << 3
}
