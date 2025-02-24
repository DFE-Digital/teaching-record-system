using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class SetInductionRequiredToCompleteMessageHandler(TrsDbContext dbContext, IClock clock) : IMessageHandler<SetInductionRequiredToCompleteMessage>
{
    public async Task HandleMessageAsync(SetInductionRequiredToCompleteMessage message)
    {
        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == message.PersonId);

        if (!InductionStatus.RequiredToComplete.IsHigherPriorityThan(person.InductionStatus))
        {
            return;
        }

        var updatedBy = message.DqtUserId is not null && message.DqtUserName is not null
            ? EventModels.RaisedByUserInfo.FromDqtUser(message.DqtUserId.Value, message.DqtUserName)
            : EventModels.RaisedByUserInfo.FromUserId(message.TrsUserId! ?? SystemUser.SystemUserId);

        person.SetInductionStatus(
            InductionStatus.RequiredToComplete,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: [],
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
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
