namespace TeachingRecordSystem.Core.Events;

public record RouteToProfessionalStatusMigratedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required EventModels.DqtInitialTeacherTraining? DqtInitialTeacherTraining { get; init; }
    public required EventModels.DqtQtsRegistration? DqtQtsRegistration { get; init; }
    public required DateOnly? DqtQtlsDate { get; init; }
    public required bool? DqtQtlsDateHasBeenSet { get; init; }
}
