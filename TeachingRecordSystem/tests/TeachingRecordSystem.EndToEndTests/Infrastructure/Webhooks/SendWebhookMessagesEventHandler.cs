using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.EndToEndTests.Infrastructure.Webhooks;

public class SendWebhookMessagesEventHandler(WebhookMessageFactory webhookMessageFactory, IWebhookSender webhookSender) : IEventHandler
{
    public async Task HandleEventAsync(IEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        var messages = await webhookMessageFactory.CreateMessagesAsync(@event);

        foreach (var message in messages)
        {
            await webhookSender.SendMessageAsync(message);
        }
    }
}
