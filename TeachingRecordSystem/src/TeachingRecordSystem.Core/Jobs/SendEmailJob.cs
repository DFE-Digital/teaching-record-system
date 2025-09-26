using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Jobs;

public class SendEmailJob(TrsDbContext dbContext, INotificationSender notificationSender, IClock clock)
{
    protected TrsDbContext DbContext => dbContext;

    protected INotificationSender NotificationSender => notificationSender;

    protected IClock Clock => clock;

    public virtual Task ExecuteAsync(Guid emailId) => SendEmailAsync(emailId);

    protected Task<Email> GetEmailByIdAsync(Guid emailId) =>
        dbContext.Emails.SingleAsync(e => e.EmailId == emailId);

    protected async Task SendEmailAsync(Guid emailId)
    {
        var email = await GetEmailByIdAsync(emailId);
        await SendEmailAsync(email);
    }

    protected async Task SendEmailAsync(Email email)
    {
        await notificationSender.SendEmailAsync(
            email.TemplateId,
            email.EmailAddress,
            new Dictionary<string, string>(email.Personalization));

        email.SentOn = clock.UtcNow;

        dbContext.AddEventWithoutBroadcast(new EmailSentEvent
        {
            EventId = Guid.NewGuid(),
            Email = EventModels.Email.FromModel(email),
            CreatedUtc = clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId
        });

        await dbContext.SaveChangesAsync();
    }
}
