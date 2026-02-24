namespace TeachingRecordSystem.Core.ApiSchema.V3;

public interface IEventMapper<TEvent, TData>
    where TEvent : IEvent
    where TData : IWebhookMessageData
{
    Task<TData?> MapEventAsync(TEvent @event);
}
