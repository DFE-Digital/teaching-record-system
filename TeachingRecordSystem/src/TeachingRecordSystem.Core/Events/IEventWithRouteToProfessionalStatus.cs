using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithRouteToProfessionalStatus
{
    RouteToProfessionalStatus RouteToProfessionalStatus { get; }
    ProfessionalStatusPersonAttributes PersonAttributes { get; }
    ProfessionalStatusPersonAttributes OldPersonAttributes { get; }
}
