using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.Events.EventHandlers;

public class CreateWebhookMessagesEventHandler(TrsDbContext dbContext, WebhookMessageFactory webhookMessageFactory) : IEventHandler
{
    public async Task HandleAsync(EventBase @event)
    {
        var messages = await webhookMessageFactory.CreateMessagesAsync(@event);
        dbContext.WebhookMessages.AddRange(messages);
        await dbContext.SaveChangesAsync();
    }
}
