using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Webhooks;

public class WebhookMessageService(IDbContextFactory<TrsDbContext> dbContextFactory, IClock clock, WebhookMessageSender messageSender) : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan[] _deliveryRetryIntervals =
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

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var messages = await dbContext.Database
                .SqlQuery<WebhookMessage>(
                    $"""
                    select * for update from webhook_messages
                    where next_delivery_attempt <= {clock.UtcNow}
                    and cancelled is null
                    """)
                .Include(m => m.WebhookEndpoint)
                .ToListAsync();

            foreach (var message in messages)
            {
                var now = clock.UtcNow;

                message.DeliveryAttempts.Add(now);

                try
                {
                    await messageSender.SendMessage(message, message.WebhookEndpoint.Address, stoppingToken);
                    message.Delivered = now;
                    message.NextDeliveryAttempt = null;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    message.DeliveryErrors.Add(ex.ToString());

                    var deliveryAttempts = message.DeliveryAttempts.Count;
                    message.NextDeliveryAttempt = deliveryAttempts < (_deliveryRetryIntervals.Length + 1) ?
                        clock.UtcNow.Add(_deliveryRetryIntervals[deliveryAttempts - 1]) :
                        null;
                }

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
                await dbContext.SaveChangesAsync();
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            }
        }
    }
}
