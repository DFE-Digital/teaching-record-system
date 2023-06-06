using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Jobs;
using QualifiedTeachersApi.Jobs.Scheduling;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

public class BatchSendQtsAwardedEmailsJobTests : JobTestBase
{
    public BatchSendQtsAwardedEmailsJobTests(JobFixture jobFixture)
        : base(jobFixture)
    {
    }

    public static TheoryData<DateTime, DateTime?, DateTime, DateTime, DateTime> DateRangeEvaluationTestData { get; } = new()
    {
        // Last awarded to date and today minus email delay (3 days) are both within GMT
        {
            new DateTime(2023, 02, 02, 23, 59, 59, DateTimeKind.Utc),
            null,
            new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 02, 03, 23, 59, 59, DateTimeKind.Utc)
        },
        // Last awarded to date is within GMT and today minus email delay (3 days) is when it switches from GMT to BST
        {
            new DateTime(2023, 03, 25, 23, 59, 59, DateTimeKind.Utc),
            null,
            new DateTime(2023, 03, 29, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 03, 26, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 03, 26, 23, 59, 59, DateTimeKind.Utc)
        },
        // Last awarded to date and today minus email delay (3 days) are both within BST
        {
            new DateTime(2023, 04, 01, 23, 59, 59, DateTimeKind.Utc),
            null,
            new DateTime(2023, 04, 05, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 04, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 04, 02, 23, 59, 59, DateTimeKind.Utc)
        },
        // Last awarded to date is within BST and today minus email delay (3 days) is when it switches from BST to GMT
        {
            new DateTime(2023, 10, 28, 23, 59, 59, DateTimeKind.Utc),
            null,
            new DateTime(2023, 11, 01, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 10, 29, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 10, 29, 23, 59, 59, DateTimeKind.Utc)
        },
        // Last awarded to date from previous job is used if available (rather than initial last awarded to date from config)
        {
            new DateTime(2022, 05, 23, 23, 59, 59, DateTimeKind.Utc),
            new DateTime(2023, 02, 02, 23, 59, 59, DateTimeKind.Utc),
            new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 02, 03, 23, 59, 59, DateTimeKind.Utc)
        },
    };

    [Theory]
    [MemberData(nameof(DateRangeEvaluationTestData))]
    public async Task Execute_ForMultipleScenarios_EvaluatesDateRangeCorrectly(
        DateTime initialLastAwardedToUtc,
        DateTime? previousJobLastAwardedToUtc,
        DateTime utcNow,
        DateTime startExpected,
        DateTime endExpected)
    {
        // Arrange
        using var dbContext = JobFixture.DbFixture.GetDbContext();
        if (previousJobLastAwardedToUtc.HasValue)
        {
            var previousJob = new QtsAwardedEmailsJob
            {
                QtsAwardedEmailsJobId = Guid.NewGuid(),
                AwardedToUtc = previousJobLastAwardedToUtc.Value,
                ExecutedUtc = utcNow.AddDays(-1)
            };
            await dbContext.QtsAwardedEmailsJobs.AddAsync(previousJob);
            await dbContext.SaveChangesAsync();
        }

        var jobOptions = Options.Create(
            new BatchSendQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        JobFixture.Clock.UtcNow = utcNow;

        DateTime startActual = DateTime.MinValue;
        DateTime endActual = DateTime.MaxValue;
        JobFixture.DataverseAdapter
            .Setup(d => d.GetQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new QtsAwardee[] { })
            .Callback<DateTime, DateTime>(
                (start, end) =>
                {
                    startActual = start;
                    endActual = end;
                });

        var job = new BatchSendQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            JobFixture.DataverseAdapter.Object,
            JobFixture.BackgroundJobScheduler.Object,
            JobFixture.Clock);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        Assert.Equal(startExpected, startActual);
        Assert.Equal(endExpected, endActual);
    }

    [Fact]
    public async Task Execute_WhenHasAwardeesForDateRange_UpdatesDatabaseAndEnqueuesJobToSendEmail()
    {
        // Arrange
        var initialLastAwardedToUtc = new DateTime(2023, 02, 02, 23, 59, 59, DateTimeKind.Utc);
        var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = JobFixture.DbFixture.GetDbContext();
        var jobOptions = Options.Create(
            new BatchSendQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        JobFixture.Clock.UtcNow = today;

        var qtsAwardee1 = new QtsAwardee
        {
            TeacherId = Guid.NewGuid(),
            Trn = "1234567",
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            EmailAddress = Faker.Internet.Email()
        };

        var qtsAwardees = new[] { qtsAwardee1 };

        JobFixture.DataverseAdapter
            .Setup(d => d.GetQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(qtsAwardees);

        var job = new BatchSendQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            JobFixture.DataverseAdapter.Object,
            JobFixture.BackgroundJobScheduler.Object,
            JobFixture.Clock);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var jobItem = await dbContext.QtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.PersonId == qtsAwardee1.TeacherId);
        Assert.NotNull(jobItem);
        Assert.Equal(qtsAwardee1.Trn, jobItem.Trn);
        Assert.Equal(qtsAwardee1.EmailAddress, jobItem.EmailAddress);
        Assert.Equal(qtsAwardee1.FirstName, jobItem.Personalization["first name"]);
        Assert.Equal(qtsAwardee1.LastName, jobItem.Personalization["last name"]);

        JobFixture.BackgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<QtsAwardedEmailJobDispatcher, Task>>>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenDoesNotHaveAwardeesForDateRange_UpdatesDatabaseOnly()
    {
        // Arrange
        var initialLastAwardedToUtc = new DateTime(2023, 02, 02, 23, 59, 59, DateTimeKind.Utc);
        var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = JobFixture.DbFixture.GetDbContext();
        var jobOptions = Options.Create(
            new BatchSendQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        JobFixture.Clock.UtcNow = today;

        JobFixture.DataverseAdapter
            .Setup(d => d.GetQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new QtsAwardee[] { });

        var job = new BatchSendQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            JobFixture.DataverseAdapter.Object,
            JobFixture.BackgroundJobScheduler.Object,
            JobFixture.Clock);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var jobInfo = await dbContext.QtsAwardedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
        Assert.NotNull(jobInfo);

        JobFixture.BackgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<QtsAwardedEmailJobDispatcher, Task>>>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenEnqueueFails_DoesNotUpdateDatabase()
    {
        // Arrange
        var initialLastAwardedToUtc = new DateTime(2023, 02, 02, 23, 59, 59, DateTimeKind.Utc);
        var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = JobFixture.DbFixture.GetDbContext();
        var jobOptions = Options.Create(
            new BatchSendQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        JobFixture.Clock.UtcNow = today;

        var qtsAwardee1 = new QtsAwardee
        {
            TeacherId = Guid.NewGuid(),
            Trn = "1234567",
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            EmailAddress = Faker.Internet.Email()
        };

        var qtsAwardees = new[] { qtsAwardee1 };

        JobFixture.DataverseAdapter
            .Setup(d => d.GetQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(qtsAwardees);

        JobFixture.BackgroundJobScheduler
            .Setup(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<QtsAwardedEmailJobDispatcher, Task>>>()))
            .Throws<Exception>();

        var job = new BatchSendQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            JobFixture.DataverseAdapter.Object,
            JobFixture.BackgroundJobScheduler.Object,
            JobFixture.Clock);

        // Act
        await Assert.ThrowsAsync<Exception>(() => job.Execute(CancellationToken.None));

        // Assert
        var jobInfo = await dbContext.QtsAwardedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
        Assert.Null(jobInfo);
        var jobItem = await dbContext.QtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.PersonId == qtsAwardee1.TeacherId);
        Assert.Null(jobItem);
    }
}
