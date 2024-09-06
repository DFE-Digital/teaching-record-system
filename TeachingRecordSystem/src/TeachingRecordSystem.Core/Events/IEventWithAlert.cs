using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithAlert
{
    Alert Alert { get; }
}
