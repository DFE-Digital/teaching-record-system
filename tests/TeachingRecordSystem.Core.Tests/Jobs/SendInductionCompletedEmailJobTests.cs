using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendInductionCompletedEmailJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_WhenCalled_GetsTrnTokenSendsEmailPublishesEventAndUpdatesDatabase()
    {
        // Arrange
        var notificationSender = new Mock<INotificationSender>();
        var inductionCompletedEmailsJobId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var trn = "1234567";
        var emailAddress = Faker.Internet.Email();
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var personalisation = new Dictionary<string, string>()
        {
            { "first name", firstName },
            { "last name", lastName }
        };

        await WithDbContextAsync(async dbContext =>
        {
            var batchJob = new InductionCompletedEmailsJob
            {
                InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
                PassedEndUtc = TimeProvider.UtcNow.AddDays(-1),
                ExecutedUtc = TimeProvider.UtcNow
            };
            dbContext.InductionCompletedEmailsJobs.Add(batchJob);

            var jobItem = new InductionCompletedEmailsJobItem
            {
                InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
                PersonId = personId,
                Trn = trn,
                EmailAddress = emailAddress,
                Personalization = personalisation
            };
            dbContext.InductionCompletedEmailsJobItems.Add(jobItem);
            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithServiceAsync<SendInductionCompletedEmailJob>(
            job => job.ExecuteAsync(inductionCompletedEmailsJobId, personId),
            notificationSender.Object);

        // Assert
        notificationSender
            .Verify(
                n => n.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s == emailAddress),
                    It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

        var updatedJobItem = await WithDbContextAsync(dbContext => dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(i =>
            i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.PersonId == personId));
        Assert.NotNull(updatedJobItem);
        Assert.True(updatedJobItem.EmailSent);

        var sentEmail = await WithDbContextAsync(dbContext => dbContext.Emails
            .SingleOrDefaultAsync(e => e.EmailAddress == emailAddress));
        Assert.NotNull(sentEmail);
        Assert.Equal(EmailTemplateIds.InductionCompletedEmailConfirmation, sentEmail.TemplateId);
        Assert.Equal(TimeProvider.UtcNow, sentEmail.SentOn);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.NotifyingInductionCompletee, x.ProcessContext.ProcessType);
            Assert.Collection(x.ProcessContext.Process.PersonIds, id => Assert.Equal(personId, id));

            Assert.Collection(
                x.Events,
                e =>
                {
                    var emailSentEvent = Assert.IsType<EmailSentEvent>(e);
                    Assert.Equal(personId, emailSentEvent.PersonId);
                    Assert.Equal(sentEmail.EmailId, emailSentEvent.Email.EmailId);
                });
        });
    }
}
