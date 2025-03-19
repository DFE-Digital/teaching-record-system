using System.Diagnostics;
using System.Transactions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class AddInductionExemptionMessageHandler(TrsDbContext dbContext, IClock clock, TrsDataSyncHelper syncHelper) :
    IMessageHandler<AddInductionExemptionMessage>
{
    public async Task HandleMessageAsync(AddInductionExemptionMessage message)
    {
        var person = await GetPersonAsync();

        if (person is null)
        {
            using (var txn = new TransactionScope(TransactionScopeOption.Suppress))
            {
                var synced = await syncHelper.SyncPersonAsync(message.PersonId, syncAudit: false);

                if (!synced)
                {
                    throw new InvalidOperationException($"Failed syncing person '{message.PersonId}'.");
                }
            }

            person = (await GetPersonAsync())!;
            Debug.Assert(person is not null);
        }

        var updatedBy = message.DqtUserId is not null && message.DqtUserName is not null
            ? EventModels.RaisedByUserInfo.FromDqtUser(message.DqtUserId.Value, message.DqtUserName)
            : EventModels.RaisedByUserInfo.FromUserId(message.TrsUserId ?? SystemUser.SystemUserId);

        person.AddInductionExemptionReason(
            message.ExemptionReasonId,
            updatedBy: updatedBy,
            now: clock.UtcNow,
            out var @event);

        if (@event is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(@event);
        }

        await dbContext.SaveChangesAsync();

        async Task<Person?> GetPersonAsync() => await dbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == message.PersonId);
    }
}
