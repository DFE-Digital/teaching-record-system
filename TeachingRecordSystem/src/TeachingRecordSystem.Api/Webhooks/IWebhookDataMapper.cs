namespace TeachingRecordSystem.Api.Webhooks;

public interface IWebhookDataMapper<TEvent> where TEvent : EventBase
{
    Task<object?> MapEvent(TEvent @event);
}
