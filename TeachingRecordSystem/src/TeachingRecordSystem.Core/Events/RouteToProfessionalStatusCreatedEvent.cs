using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record RouteToProfessionalStatusCreatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required RouteToProfessionalStatusCreatedEventChanges Changes { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required Induction Induction { get; init; }
    public required Induction OldInduction { get; init; }
}

[Flags]
public enum RouteToProfessionalStatusCreatedEventChanges
{
    None = 0,
    // Keep the following options aligned with other ProfessionalStatus events
    PersonQtlsStatus = 1 << 24,
    PersonQtsDate = 1 << 25,
    PersonEytsDate = 1 << 26,
    PersonHasEyps = 1 << 27,
    PersonPqtsDate = 1 << 28,
    PersonInductionStatus = 1 << 29,
    PersonInductionStatusWithoutExemption = 1 << 30
}
