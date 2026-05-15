using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateWebhookMessages(WebhookMessageFactory webhookMessageFactory) : IEventHandler
{
    public async Task HandleEventAsync(IEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        await webhookMessageFactory.CreateMessagesAsync(@event);
    }
}
