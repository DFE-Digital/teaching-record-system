using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendQtlsLapsedEmailJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task ExecuteAsync_SendsEmailAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));

        var email = await WithDbContextAsync(async dbContext =>
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = EmailTemplateIds.QtlsLapsed,
                EmailAddress = person.EmailAddress!,
                Personalization = new Dictionary<string, string>(),
                Metadata = new Dictionary<string, object>
                {
                    [SendQtlsLapsedEmailJob.JobMetadataKeys.PersonId] = person.PersonId
                }
            };

            dbContext.Emails.Add(email);

            await dbContext.SaveChangesAsync();

            return email;
        });

        var notificationSender = new Mock<INotificationSender>();

        // Act
        await WithServiceAsync<SendQtlsLapsedEmailJob>(job => job.ExecuteAsync(email.EmailId), notificationSender.Object);

        // Assert
        notificationSender.Verify(s => s.SendEmailAsync(
            email.TemplateId,
            person.EmailAddress!,
            It.IsAny<IReadOnlyDictionary<string, string>>()));

        var updatedEmail = await WithDbContextAsync(dbContext => dbContext.Emails.SingleAsync(e => e.EmailId == email.EmailId));
        Assert.Equal(TimeProvider.UtcNow, updatedEmail.SentOn);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.NotifyingLapsedQtlsHolder, x.ProcessContext.ProcessType);
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
