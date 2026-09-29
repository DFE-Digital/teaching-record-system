using System.Transactions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendQtlsLapsedEmailJob(TrsDbContext dbContext, IEventPublisher eventPublisher, INotificationSender notificationSender, TimeProvider timeProvider) :
    SendEmailJob(dbContext, eventPublisher, notificationSender, timeProvider)
{
    public class JobMetadataKeys
    {
        public const string PersonId = "PersonId";
    }

    public override async Task ExecuteAsync(Guid emailId)
    {
        using var txn = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var email = await base.SendEmailAsync(emailId);

        var personId = Guid.Parse(email.Metadata[JobMetadataKeys.PersonId].ToString()!);

        var processContext = new ProcessContext(ProcessType.NotifyingLapsedQtlsHolder, email.SentOn!.Value, SystemUser.SystemUserId);

        await EventPublisher.PublishSingleEventAsync(
            new EmailSentEvent
            {
                PersonId = personId,
                Email = EventModels.Email.FromModel(email)
            },
            processContext);

        txn.Complete();
    }
}
