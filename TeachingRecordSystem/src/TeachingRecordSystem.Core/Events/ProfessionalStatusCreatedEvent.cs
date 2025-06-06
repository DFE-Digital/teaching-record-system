using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record ProfessionalStatusCreatedEvent : EventBase, IEventWithPersonId, IEventWithProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required ProfessionalStatus ProfessionalStatus { get; init; }
    public required ProfessionalStatusCreatedEventChanges Changes { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required Induction Induction { get; init; }
    public required Induction OldInduction { get; init; }
}

[Flags]
public enum ProfessionalStatusCreatedEventChanges
{
    None = 0,
    // Keep the following options aligned with other ProfessionalStatus events
    PersonQtsDate = 1 << 25,
    PersonEytsDate = 1 << 26,
    PersonHasEyps = 1 << 27,
    PersonPqtsDate = 1 << 28,
    PersonInductionStatus = 1 << 29,
    InductionExemptionReasons = 1 << 30,
    PersonInductionStatusWithoutExemption = 1 << 31
}
