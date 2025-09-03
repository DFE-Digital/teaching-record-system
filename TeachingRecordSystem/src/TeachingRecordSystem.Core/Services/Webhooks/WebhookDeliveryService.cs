using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookDeliveryService(
    IWebhookSender webhookSender,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IClock clock,
    ILogger<WebhookDeliveryService> logger) : BackgroundService
{
    public const int BatchSize = 20;

    // The number of delivery errors before we log an error instead of a warning.
    private const int MessageAttemptsErrorLogThreshold = 5;

    private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);

    private static readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new Polly.Retry.RetryStrategyOptions()
        {
            BackoffType = DelayBackoffType.Linear,
            Delay = TimeSpan.FromSeconds(30),
            MaxRetryAttempts = 10
        })
        .Build();

    public static TimeSpan[] RetryIntervals { get; } =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(5),
        TimeSpan.FromHours(10),
        TimeSpan.FromHours(14),
        TimeSpan.FromHours(20),
        TimeSpan.FromHours(24),
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_pollInterval);

        do
        {
            try
            {
                await _resiliencePipeline.ExecuteAsync(
                    async ct =>
                    {
                        SendMessagesResult result;
                        do
                        {
                            result = await SendMessagesAsync(ct);
                        }
                        while (result.MoreRecords);
                    },
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task<SendMessagesResult> SendMessagesAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = clock.UtcNow;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await using var txn = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        // Get the first batch of messages that are due to be sent.
        // Constrain the batch to `batchSize`, but return one more record so we know if there are more that need to be processed.
        var messages = await dbContext.WebhookMessages
            .FromSql($"""
                select * from webhook_messages
                where next_delivery_attempt <= {clock.UtcNow}
                order by next_delivery_attempt
                limit {BatchSize + 1}
                for update skip locked
            """)
            .Where(m => m.WebhookEndpoint!.Enabled)
            .Include(m => m.WebhookEndpoint)
            .ToArrayAsync(cancellationToken);

        var moreRecords = messages.Length > BatchSize;

        await Parallel.ForEachAsync(
            messages.Take(BatchSize),
            cancellationToken,
            async (message, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var now = clock.UtcNow;
                message.DeliveryAttempts.Add(now);

                try
                {
                    await webhookSender.SendMessageAsync(message, ct);

                    message.Delivered = now;
                    message.NextDeliveryAttempt = null;
                }
                catch (Exception ex)
                {
                    var logLevel = message.DeliveryAttempts.Count >= MessageAttemptsErrorLogThreshold
                        ? LogLevel.Error
                        : LogLevel.Warning;
                    logger.Log(logLevel, ex, "Failed delivering webhook message.");

                    message.DeliveryErrors.Add(ex.Message);

                    if (message.DeliveryAttempts.Count <= RetryIntervals.Length)
                    {
                        var nextRetryInterval = RetryIntervals[message.DeliveryAttempts.Count - 1];
                        message.NextDeliveryAttempt = now.Add(nextRetryInterval);

                        // If next retry is due before we'll next be polling then ensure we return 'true' for MoreRecords.
                        // (That ensures we won't have to wait for the timer to fire again before this message is retried.)
                        var nextRun = startedAt.Add(_pollInterval);
                        if (message.NextDeliveryAttempt < nextRun)
                        {
                            moreRecords = true;
                        }
                    }
                    else
                    {
                        message.NextDeliveryAttempt = null;
                    }
                }
            });

        await dbContext.SaveChangesAsync();
        await txn.CommitAsync();

        return new(moreRecords);
    }

    public record SendMessagesResult(bool MoreRecords);
}
