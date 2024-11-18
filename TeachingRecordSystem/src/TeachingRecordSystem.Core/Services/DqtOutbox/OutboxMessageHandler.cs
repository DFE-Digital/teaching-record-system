using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Services.DqtOutbox;

public partial class OutboxMessageHandler(MessageSerializer messageSerializer, IServiceProvider serviceProvider)
{
    public async Task HandleOutboxMessageAsync(dfeta_TrsOutboxMessage outboxMessage)
    {
        var message = messageSerializer.DeserializeMessage(outboxMessage.dfeta_Payload, outboxMessage.dfeta_MessageName);

        if (message is TrnRequestMetadataMessage trnRequestMetadataMessage)
        {
            await HandleMessageAsync<TrnRequestMetadataMessage, TrnRequestMetadataMessageHandler>(trnRequestMetadataMessage);
        }
        else
        {
            throw new ArgumentException($"Unknown message type: '{outboxMessage.dfeta_MessageName}'.", nameof(outboxMessage.dfeta_MessageName));
        }

        async Task HandleMessageAsync<TMessage, THandler>(TMessage message)
            where THandler : IMessageHandler<TMessage>
        {
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using var scope = serviceScopeFactory.CreateScope();
            var handler = ActivatorUtilities.CreateInstance<THandler>(serviceProvider);
            await handler.HandleMessageAsync(message);
        }
    }
}
