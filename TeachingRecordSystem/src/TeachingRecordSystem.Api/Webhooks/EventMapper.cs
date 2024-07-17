using System.Reflection;

namespace TeachingRecordSystem.Api.Webhooks;

public class EventMapper(IKeyedServiceProvider serviceProvider)
{
    public Task<WebhookEnvelope?> MapEvent(EventBase @event, string minorVersion)
    {
        var mapper = (IEventMapper)ActivatorUtilities.CreateInstance(
            serviceProvider,
            typeof(InnerEventMapper<>).MakeGenericType(@event.GetType()));

        return mapper.MapEvent(@event, minorVersion);
    }

    private interface IEventMapper
    {
        Task<WebhookEnvelope?> MapEvent(EventBase @event, string minorVersion);
    }

    private class InnerEventMapper<TEvent>(IClock clock, IKeyedServiceProvider serviceProvider) : IEventMapper
        where TEvent : EventBase
    {
        public async Task<WebhookEnvelope?> MapEvent(EventBase @event, string minorVersion)
        {
            var webhookDataMapperType = typeof(IWebhookDataMapper<>).MakeGenericType(typeof(TEvent));
            var mapper = (IWebhookDataMapper<TEvent>?)serviceProvider.GetKeyedService(webhookDataMapperType, minorVersion);

            if (mapper is null)
            {
                return null;
            }

            var data = await mapper.MapEvent((TEvent)@event);

            if (data is null)
            {
                return null;
            }

            var eventType = data.GetType().GetCustomAttribute<CloudEventTypeAttribute>()?.EventType ??
                throw new InvalidOperationException($"{data.GetType().Name} does not have a {nameof(CloudEventTypeAttribute)}.");

            if (!eventType.StartsWith("v3."))
            {
                throw new InvalidOperationException("Event type should be prefixed with 'v3.'.");
            }
            if (!eventType.EndsWith($"+{minorVersion}"))
            {
                throw new InvalidOperationException($"Event type should be suffixed with '+{minorVersion}'.");
            }

            return new WebhookEnvelope()
            {
                CloudEventId = Guid.NewGuid().ToString(),
                CloudEventType = eventType,
                MinorVersion = minorVersion,
                TimestampUtc = clock.UtcNow,
                Data = data
            };
        }
    }
}
