using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.ApiSchema;
using TeachingRecordSystem.Core.ApiSchema.V3;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.Json;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookMessageFactory(EventMapperRegistry eventMapperRegistry, IClock clock, IMemoryCache memoryCache)
{
    private static readonly TimeSpan _webhookEndpointsCacheDuration = TimeSpan.FromMinutes(1);

    private static readonly JsonSerializerOptions _serializerOptions =
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            {
                Modifiers =
                {
                    Modifiers.OptionProperties
                }
            }
        };

    public async Task<IEnumerable<WebhookMessage>> CreateMessagesAsync(
        TrsDbContext dbContext,
        EventBase @event,
        IServiceProvider serviceProvider)
    {
        var endpoints = await memoryCache.GetOrCreateAsync(
            CacheKeys.EnabledWebhookEndpoints(),
            async e =>
            {
                e.SetAbsoluteExpiration(_webhookEndpointsCacheDuration);

                return await dbContext.WebhookEndpoints
                    .AsNoTracking()
                    .Where(e => e.Enabled && e.ApplicationUser!.Active)
                    .ToArrayAsync();
            });

        var endpointCloudEventTypeVersions = endpoints!
            .SelectMany(e =>
                e.CloudEventTypes.Select(t => (Version: e.ApiVersion, CloudEventType: t, e.WebhookEndpointId)))
            .GroupBy(t => (t.Version, t.CloudEventType), t => t.WebhookEndpointId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());

        var messages = new List<WebhookMessage>();

        foreach (var (version, cloudEventType) in endpointCloudEventTypeVersions.Keys)
        {
            Type? mapperType = null;
            Type? dataType = null;

            foreach (var v in VersionRegistry.AllV3MinorVersions.TakeUntil(e => e == version).Reverse())
            {
                mapperType = eventMapperRegistry.GetMapperType(@event.GetType(), cloudEventType, v, out dataType);

                if (mapperType is not null)
                {
                    break;
                }
            }

            if (mapperType is null)
            {
                continue;
            }

            var payload = await MapEventAsync(mapperType, dataType!);
            if (payload is null)
            {
                continue;
            }

            var serializedPayload = JsonSerializer.SerializeToElement(payload, _serializerOptions);

            messages.AddRange(endpointCloudEventTypeVersions[(version, cloudEventType)].Select(epId =>
            {
                var id = Guid.NewGuid();

                return new WebhookMessage
                {
                    WebhookMessageId = id,
                    WebhookEndpointId = epId,
                    CloudEventId = id.ToString(),
                    CloudEventType = cloudEventType,
                    Timestamp = clock.UtcNow,
                    ApiVersion = version,
                    Data = serializedPayload,
                    NextDeliveryAttempt = clock.UtcNow,
                    Delivered = null,
                    DeliveryAttempts = [],
                    DeliveryErrors = []
                };
            }));
        }

        return messages;

        Task<object?> MapEventAsync(Type mapperType, Type dataType)
        {
            var mapper = ActivatorUtilities.CreateInstance(serviceProvider, mapperType);

            var eventType = @event.GetType();

            var wrappedMapper = (IEventMapper)ActivatorUtilities.CreateInstance(
                serviceProvider,
                typeof(WrappedMapper<,>).MakeGenericType(eventType, dataType),
                mapper);

            return wrappedMapper.MapEventAsync(@event);
        }
    }

    private interface IEventMapper
    {
        Task<object?> MapEventAsync(EventBase @event);
    }

    private class WrappedMapper<TEvent, TData>(IEventMapper<TEvent, TData> innerMapper) : IEventMapper
        where TEvent : EventBase
        where TData : IWebhookMessageData
    {
        public async Task<object?> MapEventAsync(EventBase @event) =>
            await innerMapper.MapEventAsync((TEvent)@event);
    }
}
