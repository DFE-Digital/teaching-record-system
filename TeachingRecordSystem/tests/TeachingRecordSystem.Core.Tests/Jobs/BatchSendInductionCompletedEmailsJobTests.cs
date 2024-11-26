using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BatchSendInductionCompletedEmailsJobTests : InductionCompletedEmailJobTestBase
{
    public BatchSendInductionCompletedEmailsJobTests(DbFixture dbFixture)
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
            var previousJob = new InductionCompletedEmailsJob
            {
                InductionCompletedEmailsJobId = Guid.NewGuid(),
                AwardedToUtc = previousJobLastAwardedToUtc.Value,
                ExecutedUtc = utcNow.AddDays(-1)
            };
            await dbContext.InductionCompletedEmailsJobs.AddAsync(previousJob);
            await dbContext.SaveChangesAsync();
        }

        var jobOptions = Options.Create(
            new BatchSendInductionCompletedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        clock.UtcNow = utcNow;

        DateTime startActual = DateTime.MinValue;
        DateTime endActual = DateTime.MaxValue;
        dataverseAdapter
            .Setup(d => d.GetInductionCompleteesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsyncEnumerable(new InductionCompletee[] { })
            .Callback<DateTime, DateTime>(
                (start, end) =>
                {
                    startActual = start;
                    endActual = end;
                });

        var job = new BatchSendInductionCompletedEmailsJob(
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
    public Task Execute_WhenHasCompleteesForDateRange_UpdatesDatabaseAndEnqueuesJobToSendEmail() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var initialLastAwardedToUtc = new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc);
            var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
            var clock = new TestableClock();
            var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            var jobOptions = Options.Create(
                new BatchSendInductionCompletedEmailsJobOptions
                {
                    EmailDelayDays = 3,
                    InitialLastAwardedToUtc = initialLastAwardedToUtc,
                    JobSchedule = "0 8 * * *"
                });

            clock.UtcNow = today;

            var inductionCompletee1 = new InductionCompletee
            {
                TeacherId = Guid.NewGuid(),
                Trn = "1234567",
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                EmailAddress = Faker.Internet.Email()
            };

            var inductionCompletees = new[] { inductionCompletee1 };

            dataverseAdapter
                .Setup(d => d.GetInductionCompleteesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsyncEnumerable(inductionCompletees);

            var job = new BatchSendInductionCompletedEmailsJob(
                jobOptions,
                dbContext,
                dataverseAdapter.Object,
                backgroundJobScheduler.Object,
                clock);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var jobItem =
                await dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(i =>
                    i.PersonId == inductionCompletee1.TeacherId);
            Assert.NotNull(jobItem);
            Assert.Equal(inductionCompletee1.Trn, jobItem.Trn);
            Assert.Equal(inductionCompletee1.EmailAddress, jobItem.EmailAddress);
            Assert.Equal(inductionCompletee1.FirstName, jobItem.Personalization["first name"]);
            Assert.Equal(inductionCompletee1.LastName, jobItem.Personalization["last name"]);

            backgroundJobScheduler
                .Verify(
                    s => s.EnqueueAsync(It
                        .IsAny<System.Linq.Expressions.Expression<Func<InductionCompletedEmailJobDispatcher, Task>>>()),
                    Times.Once);
        });

    [Fact]
    public Task Execute_WhenDoesNotHaveCompleteesForDateRange_UpdatesDatabaseOnly() =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var initialLastAwardedToUtc = new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc);
            var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
            var clock = new TestableClock();
            var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
            var dataverseAdapter = new Mock<IDataverseAdapter>();
            var jobOptions = Options.Create(
                new BatchSendInductionCompletedEmailsJobOptions
                {
                    EmailDelayDays = 3,
                    InitialLastAwardedToUtc = initialLastAwardedToUtc,
                    JobSchedule = "0 8 * * *"
                });

            clock.UtcNow = today;

            dataverseAdapter
                .Setup(d => d.GetInductionCompleteesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsyncEnumerable(new InductionCompletee[] { });

            var job = new BatchSendInductionCompletedEmailsJob(
                jobOptions,
                dbContext,
                dataverseAdapter.Object,
                backgroundJobScheduler.Object,
                clock);

            // Act
            await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var jobInfo =
                await dbContext.InductionCompletedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
            Assert.NotNull(jobInfo);

            backgroundJobScheduler
                .Verify(
                    s => s.EnqueueAsync(It
                        .IsAny<System.Linq.Expressions.Expression<Func<InductionCompletedEmailJobDispatcher, Task>>>()),
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
                new BatchSendInductionCompletedEmailsJobOptions
                {
                    EmailDelayDays = 3,
                    InitialLastAwardedToUtc = initialLastAwardedToUtc,
                    JobSchedule = "0 8 * * *"
                });

            clock.UtcNow = today;

            var inductionCompletee1 = new InductionCompletee
            {
                TeacherId = Guid.NewGuid(),
                Trn = "1234567",
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                EmailAddress = Faker.Internet.Email()
            };

            var inductionCompletees = new[] { inductionCompletee1 };

            dataverseAdapter
                .Setup(d => d.GetInductionCompleteesForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsyncEnumerable(inductionCompletees);

            backgroundJobScheduler
                .Setup(s => s.EnqueueAsync(It
                    .IsAny<System.Linq.Expressions.Expression<Func<InductionCompletedEmailJobDispatcher, Task>>>()))
                .Throws<Exception>();

            var job = new BatchSendInductionCompletedEmailsJob(
                jobOptions,
                dbContext,
                dataverseAdapter.Object,
                backgroundJobScheduler.Object,
                clock);

            // Act
            await Assert.ThrowsAsync<Exception>(() => job.ExecuteAsync(CancellationToken.None));

            // Assert
            var jobInfo =
                await dbContext.InductionCompletedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
            Assert.Null(jobInfo);
            var jobItem =
                await dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(i =>
                    i.PersonId == inductionCompletee1.TeacherId);
            Assert.Null(jobItem);
        });
}
