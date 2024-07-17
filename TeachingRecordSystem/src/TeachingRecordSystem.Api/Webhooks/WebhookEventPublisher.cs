using NSign.Providers;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Processing;

namespace TeachingRecordSystem.Api.Webhooks;

public class WebhookEventPublisher(IDbContextFactory<TrsDbContext> dbContextFactory, EventMapper eventMapper, IClock clock) : IEventPublisher
{
    public async Task PublishEvent(EventBase @event)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var endpoints = await dbContext.Set<WebhookEndpoint>().ToListAsync();

        var endpointsByVersion = endpoints.GroupBy(e => e.MinorVersion).ToDictionary(g => g.Key, g => g.ToArray());
        var versions = endpointsByVersion.Keys;

        foreach (var version in versions)
        {
            var envelope = await eventMapper.MapEvent(@event, version);

            if (envelope is null)
            {
                continue;
            }

            dbContext.Set<WebhookMessage>().AddRange(endpointsByVersion[version].Select(e => new WebhookMessage()
            {
                WebhookMessageId = Guid.NewGuid(),
                WebhookEndpointId = e.WebhookEndpointId,
                CloudEventId = envelope.CloudEventId,
                CloudEventType = envelope.CloudEventType,
                Timestamp = envelope.TimestampUtc,
                MinorVersion = version,
                Data = envelope.Data,
                NextDeliveryAttempt = clock.UtcNow
            }));
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task Ping(Guid webhookEndpointId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var endpoint = await dbContext.Set<WebhookEndpoint>().SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);


    }
}



public class WebhookMessage
{
    public required Guid WebhookMessageId { get; init; }
    public required Guid WebhookEndpointId { get; init; }
    public virtual WebhookEndpoint WebhookEndpoint { get; } = null!;
    public required string CloudEventId { get; init; }
    public required string CloudEventType { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string MinorVersion { get; init; }
    public required object Data { get; init; }
    public required DateTime? NextDeliveryAttempt { get; set; }
    public DateTime? Delivered { get; set; }
    public List<DateTime> DeliveryAttempts { get; set; } = [];
    public List<string> DeliveryErrors { get; set; } = [];
}
