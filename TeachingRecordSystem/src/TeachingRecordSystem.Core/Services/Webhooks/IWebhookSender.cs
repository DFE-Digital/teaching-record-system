using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public interface IWebhookSender
{
    Task SendMessageAsync(WebhookMessage message, CancellationToken cancellationToken = default);
}
