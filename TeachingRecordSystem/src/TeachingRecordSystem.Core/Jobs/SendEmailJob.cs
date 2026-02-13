using System.Transactions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendEmailJob(TrsDbContext dbContext, IEventPublisher eventPublisher, INotificationSender notificationSender, IClock clock)
{
    protected TrsDbContext DbContext => dbContext;

    protected IEventPublisher EventPublisher => eventPublisher;

    protected INotificationSender NotificationSender => notificationSender;

    protected IClock Clock => clock;

    public virtual Task ExecuteAsync(Guid emailId) => SendEmailAsync(emailId);

    public virtual async Task ExecuteAsync(Guid emailId, Guid processId)
    {
        using var txn = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var processContext = await ProcessContext.FromDbAsync(dbContext, processId, clock.UtcNow);

        Guid? personId = processContext.PersonIds.Count != 0 ? processContext.PersonIds.Single() : null;

        var email = await GetEmailByIdAsync(emailId);
        await SendEmailAsync(email);

        await eventPublisher.PublishEventAsync(
            new EmailSentEvent
            {
                PersonId = personId,
                Email = EventModels.Email.FromModel(email)
            },
            processContext);

        txn.Complete();
    }

    protected async Task<Email> GetEmailByIdAsync(Guid emailId) =>
        await dbContext.Emails.FindAsync(emailId) ?? throw new InvalidOperationException($"Email with ID {emailId} not found.");

    protected async Task<Email> SendEmailAsync(Guid emailId)
    {
        var email = await GetEmailByIdAsync(emailId);
        await SendEmailAsync(email);
        return email;
    }

    protected async Task SendEmailAsync(Email email)
    {
        await notificationSender.SendEmailAsync(
            email.TemplateId,
            email.EmailAddress,
            new Dictionary<string, string>(email.Personalization));

        email.SentOn = clock.UtcNow;

        dbContext.AddEventWithoutBroadcast(new LegacyEvents.EmailSentEvent
        {
            EventId = Guid.NewGuid(),
            Email = EventModels.Email.FromModel(email),
            CreatedUtc = clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId
        });

        await dbContext.SaveChangesAsync();
    }
}
