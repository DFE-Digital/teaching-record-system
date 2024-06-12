using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Events.Processing;

public class PublishEventsBackgroundService : BackgroundService
{
    private const int BatchSize = 10;

    private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);

    private readonly IEventObserver _eventObserver;
    private readonly IDbContextFactory<TrsDbContext> _dbContextFactory;
    private readonly ILogger<PublishEventsBackgroundService> _logger;

    public PublishEventsBackgroundService(
        IEventObserver eventObserver,
        IDbContextFactory<TrsDbContext> dbContextFactory,
        ILogger<PublishEventsBackgroundService> logger)
    {
        _eventObserver = eventObserver;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_pollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishEvents(stoppingToken);
        }
    }

    // public for testing
    public async Task PublishEvents(CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // The ID and timestamp of the last processed event for this tick of the timer.
        // This is fed into the query to ensure we won't process the same event multiple times in the same timer tick
        // (which might happen otherwise if publishing the event failed first time around).
        // We cannot use timestamp alone since it's possible there are multiple events with exactly the same timestamp
        // and we don't want to miss one of them.
        DateTime lastProcessedEventTimestamp = DateTime.MinValue;
        Guid lastProcessedEventId = Guid.Empty;

        // How many events were processed in this batch.
        // If processedCount < BatchSize there are no more events to process.
        int processedCount;

        do
        {
            using var txn = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var unpublishedEvents = await dbContext.Events
                .FromSql($"""
                    select * from events
                    where published is false
                    and inserted >= {lastProcessedEventTimestamp}
                    and event_id != {lastProcessedEventId}
                    order by inserted, created
                    for update skip locked
                    limit {BatchSize}
                    """)
                .ToListAsync(cancellationToken: cancellationToken);

            if (unpublishedEvents.Count == 0)
            {
                break;
            }

            foreach (var e in unpublishedEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var eventBase = e.ToEventBase();
                    await _eventObserver.OnEventSaved(eventBase);

                    e.Published = true;

                    _logger.LogDebug("Successfully published {EventType} event {EventId}.", e.EventName, e.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish event {EventId}.", e.EventId);
                }
                finally
                {
                    lastProcessedEventTimestamp = e.Inserted;
                    lastProcessedEventId = e.EventId;
                }
            }

            await dbContext.SaveChangesAsync();
            await txn.CommitAsync();

            processedCount = unpublishedEvents.Count;

            cancellationToken.ThrowIfCancellationRequested();
        }
        while (processedCount == BatchSize);
    }
}
