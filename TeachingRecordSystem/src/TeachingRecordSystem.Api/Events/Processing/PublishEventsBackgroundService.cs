using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Events.Processing;

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

        // The ID of the last processed event for this tick of the timer.
        // This is fed into the query to ensure we won't process the same event multiple times in the same timer tick
        // (in cases where publishing the event failed).
        long lastProcessedEventId = 0;

        // How many events were processed in this batch.
        // If processedCount < BatchSize there are no more events to process.
        int processedCount;

        do
        {
            using var txn = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var unpublishedEvents = await dbContext.Events
                .FromSql($"select * from events where published is false and event_id > {lastProcessedEventId} for update skip locked limit {BatchSize}")
                .ToListAsync(cancellationToken: cancellationToken);

            if (unpublishedEvents.Count == 0)
            {
                processedCount = 0;
                continue;
            }

            foreach (var e in unpublishedEvents)
            {
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
                    lastProcessedEventId = e.EventId;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await txn.CommitAsync(cancellationToken);

            processedCount = unpublishedEvents.Count;
        }
        while (processedCount == BatchSize);
    }
}
