using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithAlert
{
    Alert Alert { get; }
}
