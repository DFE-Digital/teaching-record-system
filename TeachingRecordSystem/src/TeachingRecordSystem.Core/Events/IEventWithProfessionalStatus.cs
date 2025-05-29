using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithProfessionalStatus
{
    ProfessionalStatus ProfessionalStatus { get; }
    ProfessionalStatusPersonAttributes PersonAttributes { get; }
    ProfessionalStatusPersonAttributes OldPersonAttributes { get; }
}
