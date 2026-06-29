using Hangfire;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BatchSendProfessionalStatusEmailsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_WithQtsRouteOtherThanIqtsOrQtls_CreatesEmailWithQtsTemplateId()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var jobOptions = CreateJobOptions();

        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.AssessmentOnlyRouteId, holdsFrom: TimeProvider.Today)
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendProfessionalStatusEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var email = await WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.EmailAddress, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.QtsAwardedEmailConfirmation, email.TemplateId);
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
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId, holdsFrom: TimeProvider.Today)
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendProfessionalStatusEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var email = await WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.EmailAddress, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.InternationalQtsAwardedEmailConfirmation, email.TemplateId);
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
    public async Task Execute_WithQtlsRoute_CreatesEmailWithQtlsTemplateId()
    {
        // Arrange
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var jobOptions = CreateJobOptions();

        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.QtlsAndSetMembershipId, holdsFrom: TimeProvider.Today)
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendProfessionalStatusEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var email = await WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.EmailAddress, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.QtlsPostLaunchForAllUsers, email.TemplateId);
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
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus, holdsFrom: TimeProvider.Today)
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendProfessionalStatusEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var email = await WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.EmailAddress, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.EytsAwardedEmailConfirmation, email.TemplateId);
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
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.QtlsAndSetMembershipId, holdsFrom: TimeProvider.Today)
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            var route = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
            route.Delete(
                allRouteTypes: await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                additionalInformation: null,
                out var deletedEvent);
            dbContext.AddEventWithoutBroadcast(deletedEvent);
            await dbContext.SaveChangesAsync();
        });

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendProfessionalStatusEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var email = await WithDbContextAsync(dbContext => dbContext.Emails.SingleOrDefaultAsync());
        Assert.NotNull(email);
        Assert.Equal(person.EmailAddress, email.EmailAddress);
        Assert.Equal(EmailTemplateIds.QtlsLapsed, email.TemplateId);

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
                InitialLastHoldsFromEndUtc = TimeProvider.Today.AddDays(-5).ToDateTime(),
                JobSchedule = Cron.Never(),
                RaisedByUserIds = [SystemUser.SystemUserId]
            });
}
