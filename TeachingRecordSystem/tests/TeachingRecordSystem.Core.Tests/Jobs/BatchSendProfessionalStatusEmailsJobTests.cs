using Hangfire;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BatchSendProfessionalStatusEmailsJobTests(NightlyEmailJobFixture dbFixture) : NightlyEmailJobTestBase(dbFixture)
{
    [Fact]
    public async Task Execute_WithQtsRouteOtherThanIqts_CreatesEmailWithQtsTemplateId()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var jobOptions = CreateJobOptions();

        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.QtlsAndSetMembershipId, holdsFrom: Clock.Today)
            .WithEmail(TestData.GenerateUniqueEmail()));

        Clock.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        var job = new BatchSendProfessionalStatusEmailsJob(
            jobOptions,
            Fixture.DbFixture.GetDbContextFactory(),
            backgroundJobScheduler.Object,
            Clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var email = await DbFixture.WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.Email, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.QtsAwardedEmailConfirmationTemplateId, email.TemplateId);
        Assert.Equal(person.FirstName, email.Personalization["first name"]);
        Assert.Equal(person.LastName, email.Personalization["last name"]);
        Assert.Equal(person.Trn, email.Metadata["Trn"].ToString());

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendAytqInviteEmailJob, Task>>>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WithIqtsRoute_CreatesEmailWithIqtsTemplateId()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var jobOptions = CreateJobOptions();

        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId, holdsFrom: Clock.Today)
            .WithEmail(TestData.GenerateUniqueEmail()));

        Clock.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        var job = new BatchSendProfessionalStatusEmailsJob(
            jobOptions,
            Fixture.DbFixture.GetDbContextFactory(),
            backgroundJobScheduler.Object,
            Clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var email = await DbFixture.WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.Email, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.InternationalQtsAwardedEmailConfirmationTemplateId, email.TemplateId);
        Assert.Equal(person.FirstName, email.Personalization["first name"]);
        Assert.Equal(person.LastName, email.Personalization["last name"]);
        Assert.Equal(person.Trn, email.Metadata["Trn"].ToString());

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendAytqInviteEmailJob, Task>>>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WithEytsRoute_CreatesEmailWithEytsTemplateId()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var jobOptions = CreateJobOptions();

        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus, holdsFrom: Clock.Today)
            .WithEmail(TestData.GenerateUniqueEmail()));

        Clock.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        var job = new BatchSendProfessionalStatusEmailsJob(
            jobOptions,
            Fixture.DbFixture.GetDbContextFactory(),
            backgroundJobScheduler.Object,
            Clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var email = await DbFixture.WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.Email, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.EytsAwardedEmailConfirmationTemplateId, email.TemplateId);
        Assert.Equal(person.FirstName, email.Personalization["first name"]);
        Assert.Equal(person.LastName, email.Personalization["last name"]);
        Assert.Equal(person.Trn, email.Metadata["Trn"].ToString());

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendAytqInviteEmailJob, Task>>>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_WithLostQtls_CreatesEmailWithExpiredQtlsTemplateId()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var jobOptions = CreateJobOptions();

        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.QtlsAndSetMembershipId, holdsFrom: Clock.Today)
            .WithEmail(TestData.GenerateUniqueEmail()));

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            var route = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
            route.Delete(
                allRouteTypes: await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out var deletedEvent);
            dbContext.AddEventWithoutBroadcast(deletedEvent);
            await dbContext.SaveChangesAsync();
        });

        Clock.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        var job = new BatchSendProfessionalStatusEmailsJob(
            jobOptions,
            Fixture.DbFixture.GetDbContextFactory(),
            backgroundJobScheduler.Object,
            Clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var email = await DbFixture.WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.Email, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.QtlsLapsedTemplateId, email.TemplateId);

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendEmailJob, Task>>>()),
                Times.Once);
    }

    private IOptions<BatchSendProfessionalStatusEmailsOptions> CreateJobOptions() =>
        Options.Create(
            new BatchSendProfessionalStatusEmailsOptions
            {
                EmailDelayDays = 3,
                InitialLastHoldsFromEndUtc = Clock.Today.AddDays(-5).ToDateTime(),
                JobSchedule = Cron.Never(),
                RaisedByUserIds = [SystemUser.SystemUserId]
            });
}
