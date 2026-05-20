using Hangfire;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class ScheduleTrnRecipientEmailsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_SchedulesExpectedEmails()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();

        // Person created from an application user that we should be sending an email to
        var applicationUser1 = await TestData.CreateApplicationUserAsync();
        var personFromApplicationUser1 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress()
            .WithTrnRequest(applicationUser1.UserId, requestId: Guid.NewGuid().ToString()));

        // Person created from an application user that we should NOT be sending an email to
        var applicationUser2 = await TestData.CreateApplicationUserAsync();
        var personFromApplicationUser2 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress()
            .WithTrnRequest(applicationUser2.UserId, requestId: Guid.NewGuid().ToString()));

        var options = Options.Create(new ScheduleTrnRecipientEmailsJobOptions
        {
            EarliestRecordCreationDate = DateOnly.FromDateTime(new DateTime(2021, 1, 4, 0, 0, 0, DateTimeKind.Utc).AddDays(-1).Date),
            JobSchedule = Cron.Never(),
            RequestedByUserIds = [applicationUser1.UserId],
            EmailDelayDays = 1
        });

        Clock.Advance(TimeSpan.FromDays(options.Value.EmailDelayDays + 1));

        // Act
        await WithServiceAsync<ScheduleTrnRecipientEmailsJob>(job => job.ExecuteAsync(CancellationToken.None), options, backgroundJobScheduler.Object);

        // Assert
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.ToArrayAsync());

        Assert.DoesNotContain(emails, e => e.EmailAddress == personFromApplicationUser2.EmailAddress);

        Assert.Collection(
            emails,
            email =>
            {
                Assert.Equal(personFromApplicationUser1.EmailAddress, email.EmailAddress);
                Assert.Equal(EmailTemplateIds.TraineeTrnRecipient, email.TemplateId);
                Assert.Equal($"{personFromApplicationUser1.FirstName} {personFromApplicationUser1.LastName}", email.Personalization["name"]);
                Assert.Equal(personFromApplicationUser1.Trn, email.Personalization["TRN"].ToString());
            });

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendTrnRecipientEmailJob, Task>>>()),
                Times.Once);
    }
}
