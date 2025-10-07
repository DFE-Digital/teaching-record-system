using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithMandatoryQualification
{
    MandatoryQualification MandatoryQualification { get; }
}
