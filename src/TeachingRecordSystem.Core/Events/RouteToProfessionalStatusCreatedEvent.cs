namespace TeachingRecordSystem.Core.Events;

public record RouteToProfessionalStatusCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required RouteToProfessionalStatusCreatedEventChanges Changes { get; init; }
    public required EventModels.ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required EventModels.ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required EventModels.Induction Induction { get; init; }
    public required EventModels.Induction OldInduction { get; init; }
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
