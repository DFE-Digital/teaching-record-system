using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record DqtInitialTeacherTrainingCreatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required DqtInitialTeacherTraining? InitialTeacherTraining { get; init; }
}
