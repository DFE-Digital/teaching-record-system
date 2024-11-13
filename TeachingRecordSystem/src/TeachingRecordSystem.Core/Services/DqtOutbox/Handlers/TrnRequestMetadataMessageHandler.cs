using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class TrnRequestMetadataMessageHandler(TrnRequestHelper trnRequestHelper, TrsDbContext dbContext) : IMessageHandler<TrnRequestMetadataMessage>
{
    public async Task HandleMessage(TrnRequestMetadataMessage message)
    {
        var trnRequest = await trnRequestHelper.GetTrnRequestInfo(message.ApplicationUserId, message.RequestId) ??
            throw new InvalidOperationException($"TRN request does not exist.\nUser ID: '{message.ApplicationUserId}'.\nRequest ID: '{message.RequestId}'.");

        // There are two possibilities here; either the TRN request is already Completed or it's still Pending.
        // If it's completed we need to attach the One Login user immediately but if it's Pending
        // we can't process it yet (since we don't know which record to attach to) so we stash the data in the
        // TrnRequestMetadata table for the GET endpoint to process when it's polled for the outcome.

        if (trnRequest.Completed)
        {
            await trnRequestHelper.EnsureOneLoginUserIsConnected(trnRequest, message.VerifiedOneLoginUserSubject);
        }
        else
        {
            try
            {
                dbContext.TrnRequestMetadata.Add(new DataStore.Postgres.Models.TrnRequestMetadata()
                {
                    ApplicationUserId = message.ApplicationUserId,
                    RequestId = message.RequestId,
                    VerifiedOneLoginUserSubject = message.VerifiedOneLoginUserSubject
                });
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dex) when (dex.InnerException is PostgresException { SqlState: "23505" })
            {
                // Already added
            }
        }
    }
}
