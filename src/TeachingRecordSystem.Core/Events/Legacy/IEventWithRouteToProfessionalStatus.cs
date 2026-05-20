using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithRouteToProfessionalStatus
{
    RouteToProfessionalStatus RouteToProfessionalStatus { get; }
    ProfessionalStatusPersonAttributes PersonAttributes { get; }
    ProfessionalStatusPersonAttributes OldPersonAttributes { get; }
}
