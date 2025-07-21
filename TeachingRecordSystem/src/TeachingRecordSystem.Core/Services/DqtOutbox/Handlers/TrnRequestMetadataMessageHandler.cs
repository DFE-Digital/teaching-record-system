using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class TrnRequestMetadataMessageHandler(TrsDbContext dbContext) : IMessageHandler<TrnRequestMetadataMessage>
{
    public async Task HandleMessageAsync(TrnRequestMetadataMessage message)
    {
        if (!await dbContext.TrnRequestMetadata.AnyAsync(m => m.ApplicationUserId == message.ApplicationUserId && m.RequestId == message.RequestId))
        {
            var trnRequestMetadata = DataStore.Postgres.Models.TrnRequestMetadata.FromOutboxMessage(message);

            if (message.ResolvedPersonId is Guid personId)
            {
                trnRequestMetadata.SetResolvedPerson(personId);
            }

            dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
            await dbContext.SaveChangesAsync();
        }
    }
}
