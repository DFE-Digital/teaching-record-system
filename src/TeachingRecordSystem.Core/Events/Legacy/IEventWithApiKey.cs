using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithApiKey
{
    ApiKey ApiKey { get; }
}
