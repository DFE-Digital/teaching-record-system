using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithApiKey
{
    ApiKey ApiKey { get; }
}
