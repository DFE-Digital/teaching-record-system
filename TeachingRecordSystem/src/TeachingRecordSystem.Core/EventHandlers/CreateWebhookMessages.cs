using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateWebhookMessages(TrsDbContext dbContext, IServiceProvider serviceProvider) : IEventHandler
{
    public async Task HandleEventAsync(IEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        var webhookMessageFactory = serviceProvider.GetRequiredService<WebhookMessageFactory>();
        var messages = await webhookMessageFactory.CreateMessagesAsync(dbContext, @event, serviceProvider);

        if (messages.Any())
        {
            dbContext.WebhookMessages.AddRange(messages);
            await dbContext.SaveChangesAsync();
        }
    }
}
