using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateWebhookMessages(
    TrsDbContext dbContext,
    WebhookMessageFactory webhookMessageFactory) : IEventHandler
{
    public async Task HandleEventAsync(IEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        var messages = await webhookMessageFactory.CreateMessagesAsync(@event);

        if (messages.Any())
        {
            dbContext.WebhookMessages.AddRange(messages);
            await dbContext.SaveChangesAsync();
        }
    }
}
