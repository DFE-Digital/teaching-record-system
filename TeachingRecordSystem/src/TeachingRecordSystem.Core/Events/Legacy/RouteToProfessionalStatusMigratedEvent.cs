using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record RouteToProfessionalStatusMigratedEvent : EventBase, IEventWithPersonId, IEventWithRouteToProfessionalStatus
{
    public required Guid PersonId { get; init; }
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required DqtInitialTeacherTraining? DqtInitialTeacherTraining { get; init; }
    public required DqtQtsRegistration? DqtQtsRegistration { get; init; }
    public required DateOnly? DqtQtlsDate { get; init; }
    public required bool? DqtQtlsDateHasBeenSet { get; init; }
}
