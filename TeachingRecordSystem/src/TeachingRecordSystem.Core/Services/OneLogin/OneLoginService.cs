using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public class OneLoginService(
    TrsDbContext dbContext,
    INotificationSender notificationSender,
    IBackgroundJobScheduler backgroundJobScheduler)
{
    public Task<string> GetRecordNotFoundEmailContentAsync(string personName)
    {
        return notificationSender.RenderEmailTemplateAsync(
            EmailTemplateIds.OneLoginCannotFindRecord,
            GetOneLoginCannotFindRecordEmailPersonalization(personName));
    }

    public async Task EnqueueRecordNotFoundEmailAsync(string emailAddress, string personName, ProcessContext processContext)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.OneLoginCannotFindRecord,
            EmailAddress = emailAddress,
            Personalization = GetOneLoginCannotFindRecordEmailPersonalization(personName).ToDictionary()
        };

        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
    }

    public async Task SetUserVerifiedAsync(SetUserVerifiedOptions options, ProcessContext processContext)
    {
        var user = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == options.OneLoginUserSubject);

        if (user.VerifiedOn is not null)
        {
            throw new InvalidOperationException("User is already verified.");
        }

        user.SetVerified(
            processContext.Now,
            options.VerificationRoute,
            verifiedByApplicationUserId: null,
            options.VerifiedNames,
            options.VerifiedDatesOfBirth);

        await dbContext.SaveChangesAsync();

        // TODO Emit an event when we've figured out what they should look like
    }

    public async Task SetUserMatchedAsync(SetUserMatchedOptions options, ProcessContext processContext)
    {
        var user = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == options.OneLoginUserSubject);

        if (user.VerifiedOn is null)
        {
            throw new InvalidOperationException("User must be verified.");
        }

        user.SetMatched(processContext.Now, options.MatchedPersonId, options.MatchRoute, options.MatchedAttributes);

        await dbContext.SaveChangesAsync();

        // TODO Emit an event when we've figured out what they should look like
    }

    private static IReadOnlyDictionary<string, string> GetOneLoginCannotFindRecordEmailPersonalization(string personName) =>
        new Dictionary<string, string> { ["name"] = personName };
}
