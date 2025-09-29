using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Core.ApiSchema.V3;

public interface IEventMapper<TEvent, TData>
    where TEvent : EventBase
    where TData : IWebhookMessageData
{
    Task<TData?> MapEventAsync(TEvent @event);
}
