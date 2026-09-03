using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendAytqInviteEmailJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Theory]
    [InlineData(EmailTemplateIds.QtsAwardedEmailConfirmation, ProcessType.NotifyingQtsAwardee)]
    [InlineData(EmailTemplateIds.QtlsPostLaunchForAllUsers, ProcessType.NotifyingQtlsAwardee)]
    [InlineData(EmailTemplateIds.InternationalQtsAwardedEmailConfirmation, ProcessType.NotifyingInternationalQtsAwardee)]
    [InlineData(EmailTemplateIds.EytsAwardedEmailConfirmation, ProcessType.NotifyingEytsAwardee)]
    public async Task Execute_WhenCalled_GetsTrnTokenSendsEmailPublishesEventAndUpdatesDatabase(
        string templateId,
        ProcessType expectedProcessType)
    {
        // Arrange
        var notificationSender = new Mock<INotificationSender>();

        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));

        var email = await WithDbContextAsync(async dbContext =>
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = templateId,
                EmailAddress = person.EmailAddress!,
                Personalization = new Dictionary<string, string>
                {
                    ["first name"] = person.FirstName,
                    ["last name"] = person.LastName
                },
                Metadata = new Dictionary<string, object>
                {
                    [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = person.Trn,
                    [SendAytqInviteEmailJob.JobMetadataKeys.PersonId] = person.PersonId
                },
                SentOn = null
            };

            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();
            return email;
        });

        // Act
        await WithServiceAsync<SendAytqInviteEmailJob>(job => job.ExecuteAsync(email.EmailId), notificationSender.Object);

        // Assert
        notificationSender
            .Verify(
                n => n.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s == person.EmailAddress),
                    It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

        var updatedEmail = await WithDbContextAsync(dbContext => dbContext.Emails.SingleAsync(e => e.EmailId == email.EmailId));
        Assert.Equal(TimeProvider.UtcNow, updatedEmail.SentOn);

        var events = await WithDbContextAsync(dbContext => dbContext.Events
            .Where(e => e.EventName == nameof(LegacyEvents.EmailSentEvent))
            .ToListAsync());

        var legacyEmailSentEvent = events
            .Select(e => (LegacyEvents.EmailSentEvent)e.ToEventBase())
            .SingleOrDefault(e => e.Email.EmailId == email.EmailId);

        Assert.NotNull(legacyEmailSentEvent);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(expectedProcessType, x.ProcessContext.ProcessType);
            Assert.Collection(x.ProcessContext.Process.PersonIds, id => Assert.Equal(person.PersonId, id));

            Assert.Collection(
                x.Events,
                e =>
                {
                    var emailSentEvent = Assert.IsType<EmailSentEvent>(e);
                    Assert.Equal(person.PersonId, emailSentEvent.PersonId);
                    Assert.Equal(email.EmailId, emailSentEvent.Email.EmailId);
                });
        });
    }
}
