using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class RemoveInductionExemptionMessageHandler(TrsDbContext dbContext, IClock clock) : IMessageHandler<RemoveInductionExemptionMessage>
{
    public async Task HandleMessageAsync(RemoveInductionExemptionMessage message)
    {
        var person = await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleAsync(p => p.PersonId == message.PersonId);

        var updatedBy = message.DqtUserId is not null && message.DqtUserName is not null
            ? EventModels.RaisedByUserInfo.FromDqtUser(message.DqtUserId.Value, message.DqtUserName)
            : EventModels.RaisedByUserInfo.FromUserId(message.TrsUserId ?? SystemUser.SystemUserId);

        person.RemoveInductionExemptionReason(
            message.ExemptionReasonId,
            updatedBy: updatedBy,
            now: clock.UtcNow,
            out var @event);

        if (@event is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(@event);
        }

        await dbContext.SaveChangesAsync();
    }
}
