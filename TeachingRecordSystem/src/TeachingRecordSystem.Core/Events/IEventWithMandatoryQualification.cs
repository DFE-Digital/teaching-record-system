using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithMandatoryQualification
{
    MandatoryQualification MandatoryQualification { get; }
}
