using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BatchSendEytsAwardedEmailsJobTests : EytsAwardedEmailJobTestBase
{
    public BatchSendEytsAwardedEmailsJobTests(DbFixture dbFixture)
        : base(dbFixture)
    {
    }

    public static TheoryData<DateTime, DateTime?, DateTime, DateTime, DateTime> DateRangeEvaluationTestData { get; } = new()
    {
        // Last awarded to date and today minus email delay (3 days) are both within GMT
        {
            new DateTime(2023, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            null,
            new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc)
        },
        // Last awarded to date is within GMT and today minus email delay (3 days) is when it switches from GMT to BST
        {
            new DateTime(2023, 03, 26, 0, 0, 0, DateTimeKind.Utc),
            null,
            new DateTime(2023, 03, 30, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 03, 26, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 03, 27, 0, 0, 0, DateTimeKind.Utc)
        },
        // Last awarded to date and today minus email delay (3 days) are both within BST
        {
            new DateTime(2023, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            null,
            new DateTime(2023, 04, 05, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 04, 01, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 04, 02, 0, 0, 0, DateTimeKind.Utc)
        },
        // Last awarded to date is within BST and today minus email delay (3 days) is when it switches from BST to GMT
        {
            new DateTime(2023, 10, 29, 0, 0, 0, DateTimeKind.Utc),
            null,
            new DateTime(2023, 11, 02, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 10, 29, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 10, 30, 0, 0, 0, DateTimeKind.Utc)
        },
        // Last awarded to date from previous job is used if available (rather than initial last awarded to date from config)
        {
            new DateTime(2022, 05, 23, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc),
            new DateTime(2023, 02, 02, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc)
        },
    };

    [Theory]
    [MemberData(nameof(DateRangeEvaluationTestData))]
    public Task Execute_ForMultipleScenarios_EvaluatesDateRangeCorrectly(
        DateTime initialLastAwardedToUtc,
        DateTime? previousJobLastAwardedToUtc,
        DateTime utcNow,
        DateTime startExpected,
        DateTime endExpected) => DbFixture.WithDbContextAsync(async dbContext =>
    {
        // Arrange
        var clock = new TestableClock();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var dataverseAdapter = new Mock<IDataverseAdapter>();
        if (previousJobLastAwardedToUtc.HasValue)
        {
            var previousJob = new EytsAwardedEmailsJob
            {
                EytsAwardedEmailsJobId = Guid.NewGuid(),
                AwardedToUtc = previousJobLastAwardedToUtc.Value,
                ExecutedUtc = utcNow.AddDays(-1)
            };
            await dbContext.EytsAwardedEmailsJobs.AddAsync(previousJob);
            await dbContext.SaveChangesAsync();
        }

        var jobOptions = Options.Create(
            new BatchSendEytsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        clock.UtcNow = utcNow;

        DateTime startActual = DateTime.MinValue;
        DateTime endActual = DateTime.MaxValue;
        dataverseAdapter
            .Setup(d => d.GetEytsAwardeesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsyncEnumerable(new EytsAwardee[] { })
            .Callback<DateTime, DateTime>(
                (start, end) =>
                {
                    startActual = start;
                    endActual = end;
                });

        var job = new BatchSendEytsAwardedEmailsJob(
            jobOptions,
            dbContext,
            dataverseAdapter.Object,
            backgroundJobScheduler.Object,
            clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.Equal(startExpected, startActual);
        Assert.Equal(endExpected, endActual);
    });

    [Fact]
    public Task Execute_WhenHasAwardeesForDateRange_UpdatesDatabaseAndEnqueuesJobToSendEmail() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var initialLastAwardedToUtc = new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc);
            var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
            var clock = new TestableClock();
            var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            var jobOptions = Options.Create(
                new BatchSendEytsAwardedEmailsJobOptions
                {
                    EmailDelayDays = 3,
                    InitialLastAwardedToUtc = initialLastAwardedToUtc,
                    JobSchedule = "0 8 * * *"
                });

            clock.UtcNow = today;

            var eytsAwardee1 = new EytsAwardee
            {
                TeacherId = Guid.NewGuid(),
                Trn = "1234567",
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                EmailAddress = Faker.Internet.Email()
            };

            var eytsAwardees = new[] { eytsAwardee1 };

            dataverseAdapter
                .Setup(d => d.GetEytsAwardeesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsyncEnumerable(eytsAwardees);

            var job = new BatchSendEytsAwardedEmailsJob(
                jobOptions,
                dbContext,
                dataverseAdapter.Object,
                backgroundJobScheduler.Object,
                clock);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var jobItem =
                await dbContext.EytsAwardedEmailsJobItems.SingleOrDefaultAsync(
                    i => i.PersonId == eytsAwardee1.TeacherId);
            Assert.NotNull(jobItem);
            Assert.Equal(eytsAwardee1.Trn, jobItem.Trn);
            Assert.Equal(eytsAwardee1.EmailAddress, jobItem.EmailAddress);
            Assert.Equal(eytsAwardee1.FirstName, jobItem.Personalization["first name"]);
            Assert.Equal(eytsAwardee1.LastName, jobItem.Personalization["last name"]);

            backgroundJobScheduler
                .Verify(
                    s => s.EnqueueAsync(It
                        .IsAny<System.Linq.Expressions.Expression<Func<EytsAwardedEmailJobDispatcher, Task>>>()),
                    Times.Once);
        });

    [Fact]
    public Task Execute_WhenDoesNotHaveAwardeesForDateRange_UpdatesDatabaseOnly() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var initialLastAwardedToUtc = new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc);
            var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
            var clock = new TestableClock();
            var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            var jobOptions = Options.Create(
                new BatchSendEytsAwardedEmailsJobOptions
                {
                    EmailDelayDays = 3,
                    InitialLastAwardedToUtc = initialLastAwardedToUtc,
                    JobSchedule = "0 8 * * *"
                });

            clock.UtcNow = today;

            dataverseAdapter
                .Setup(d => d.GetEytsAwardeesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsyncEnumerable(new EytsAwardee[] { });

            var job = new BatchSendEytsAwardedEmailsJob(
                jobOptions,
                dbContext,
                dataverseAdapter.Object,
                backgroundJobScheduler.Object,
                clock);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var jobInfo = await dbContext.EytsAwardedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
            Assert.NotNull(jobInfo);

            backgroundJobScheduler
                .Verify(
                    s => s.EnqueueAsync(It
                        .IsAny<System.Linq.Expressions.Expression<Func<EytsAwardedEmailJobDispatcher, Task>>>()),
                    Times.Never);
        });

    [Fact]
    public Task Execute_WhenEnqueueFails_DoesNotUpdateDatabase() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var initialLastAwardedToUtc = new DateTime(2023, 02, 02, 0, 0, 0, DateTimeKind.Utc);
            var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
            var clock = new TestableClock();
            var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            var jobOptions = Options.Create(
                new BatchSendEytsAwardedEmailsJobOptions
                {
                    EmailDelayDays = 3,
                    InitialLastAwardedToUtc = initialLastAwardedToUtc,
                    JobSchedule = "0 8 * * *"
                });

            clock.UtcNow = today;

            var eytsAwardee1 = new EytsAwardee
            {
                TeacherId = Guid.NewGuid(),
                Trn = "1234567",
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                EmailAddress = Faker.Internet.Email()
            };

            var eytsAwardees = new[] { eytsAwardee1 };

            dataverseAdapter
                .Setup(d => d.GetEytsAwardeesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsyncEnumerable(eytsAwardees);

            backgroundJobScheduler
                .Setup(s => s.EnqueueAsync(It
                    .IsAny<System.Linq.Expressions.Expression<Func<EytsAwardedEmailJobDispatcher, Task>>>()))
                .Throws<Exception>();

            var job = new BatchSendEytsAwardedEmailsJob(
                jobOptions,
                dbContext,
                dataverseAdapter.Object,
                backgroundJobScheduler.Object,
                clock);

            // Act
            await Assert.ThrowsAsync<Exception>(() => job.ExecuteAsync(CancellationToken.None));

            // Assert
            var jobInfo = await dbContext.EytsAwardedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
            Assert.Null(jobInfo);
            var jobItem =
                await dbContext.EytsAwardedEmailsJobItems.SingleOrDefaultAsync(
                    i => i.PersonId == eytsAwardee1.TeacherId);
            Assert.Null(jobItem);
        });
}
