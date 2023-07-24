﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Jobs;
using TeachingRecordSystem.Api.Jobs.Scheduling;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Dqt;

namespace TeachingRecordSystem.Api.Tests.Jobs;

public class BatchSendInternationalQtsAwardedEmailsJobTests : InternationalQtsAwardedEmailJobTestBase
{
    public BatchSendInternationalQtsAwardedEmailsJobTests(DbFixture dbFixture)
        : base(dbFixture)
    {
    }

    public static TestArguments<DateTime, DateTime?, DateTime, DateTime, DateTime> DateRangeEvaluationTestData { get; } = new()
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

    [Test]
    [MemberData(nameof(DateRangeEvaluationTestData))]
    public async Task Execute_ForMultipleScenarios_EvaluatesDateRangeCorrectly(
        DateTime initialLastAwardedToUtc,
        DateTime? previousJobLastAwardedToUtc,
        DateTime utcNow,
        DateTime startExpected,
        DateTime endExpected)
    {
        // Arrange
        using var dbContext = DbFixture.GetDbContext();
        var clock = new TestableClock();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var dataverseAdapter = new Mock<IDataverseAdapter>();
        if (previousJobLastAwardedToUtc.HasValue)
        {
            var previousJob = new InternationalQtsAwardedEmailsJob
            {
                InternationalQtsAwardedEmailsJobId = Guid.NewGuid(),
                AwardedToUtc = previousJobLastAwardedToUtc.Value,
                ExecutedUtc = utcNow.AddDays(-1)
            };
            await dbContext.InternationalQtsAwardedEmailsJobs.AddAsync(previousJob);
            await dbContext.SaveChangesAsync();
        }

        var jobOptions = Options.Create(
            new BatchSendInternationalQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        clock.UtcNow = utcNow;

        DateTime startActual = DateTime.MinValue;
        DateTime endActual = DateTime.MaxValue;
        dataverseAdapter
            .Setup(d => d.GetInternationalQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsyncEnumerable(new InternationalQtsAwardee[] { })
            .Callback<DateTime, DateTime>(
                (start, end) =>
                {
                    startActual = start;
                    endActual = end;
                });

        var job = new BatchSendInternationalQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            dataverseAdapter.Object,
            backgroundJobScheduler.Object,
            clock);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        Assert.Equal(startExpected, startActual);
        Assert.Equal(endExpected, endActual);
    }

    [Test]
    public async Task Execute_WhenHasAwardeesForDateRange_UpdatesDatabaseAndEnqueuesJobToSendEmail()
    {
        // Arrange
        var initialLastAwardedToUtc = new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc);
        var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = DbFixture.GetDbContext();
        var clock = new TestableClock();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var dataverseAdapter = new Mock<IDataverseAdapter>();
        var jobOptions = Options.Create(
            new BatchSendInternationalQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        clock.UtcNow = today;

        var qtsAwardee1 = new InternationalQtsAwardee
        {
            TeacherId = Guid.NewGuid(),
            Trn = "1234567",
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            EmailAddress = Faker.Internet.Email()
        };

        var qtsAwardees = new[] { qtsAwardee1 };

        dataverseAdapter
            .Setup(d => d.GetInternationalQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsyncEnumerable(qtsAwardees);

        var job = new BatchSendInternationalQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            dataverseAdapter.Object,
            backgroundJobScheduler.Object,
            clock);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var jobItem = await dbContext.InternationalQtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.PersonId == qtsAwardee1.TeacherId);
        Assert.NotNull(jobItem);
        Assert.Equal(qtsAwardee1.Trn, jobItem.Trn);
        Assert.Equal(qtsAwardee1.EmailAddress, jobItem.EmailAddress);
        Assert.Equal(qtsAwardee1.FirstName, jobItem.Personalization["first name"]);
        Assert.Equal(qtsAwardee1.LastName, jobItem.Personalization["last name"]);

        backgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<InternationalQtsAwardedEmailJobDispatcher, Task>>>()), Times.Once);
    }

    [Test]
    public async Task Execute_WhenDoesNotHaveAwardeesForDateRange_UpdatesDatabaseOnly()
    {
        // Arrange
        var initialLastAwardedToUtc = new DateTime(2023, 02, 03, 0, 0, 0, DateTimeKind.Utc);
        var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = DbFixture.GetDbContext();
        var clock = new TestableClock();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var dataverseAdapter = new Mock<IDataverseAdapter>();
        var jobOptions = Options.Create(
            new BatchSendInternationalQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        clock.UtcNow = today;

        dataverseAdapter
            .Setup(d => d.GetInternationalQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsyncEnumerable(Array.Empty<InternationalQtsAwardee>());

        var job = new BatchSendInternationalQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            dataverseAdapter.Object,
            backgroundJobScheduler.Object,
            clock);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var jobInfo = await dbContext.InternationalQtsAwardedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
        Assert.NotNull(jobInfo);

        backgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<InternationalQtsAwardedEmailJobDispatcher, Task>>>()), Times.Never);
    }

    [Test]
    public async Task Execute_WhenEnqueueFails_DoesNotUpdateDatabase()
    {
        // Arrange
        var initialLastAwardedToUtc = new DateTime(2023, 02, 02, 0, 0, 0, DateTimeKind.Utc);
        var today = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = DbFixture.GetDbContext();
        var clock = new TestableClock();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var dataverseAdapter = new Mock<IDataverseAdapter>();
        var jobOptions = Options.Create(
            new BatchSendInternationalQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        clock.UtcNow = today;

        var qtsAwardee1 = new InternationalQtsAwardee
        {
            TeacherId = Guid.NewGuid(),
            Trn = "1234567",
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            EmailAddress = Faker.Internet.Email()
        };

        var qtsAwardees = new[] { qtsAwardee1 };

        dataverseAdapter
            .Setup(d => d.GetInternationalQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsyncEnumerable(qtsAwardees);

        backgroundJobScheduler
            .Setup(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<InternationalQtsAwardedEmailJobDispatcher, Task>>>()))
            .Throws<Exception>();

        var job = new BatchSendInternationalQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            dataverseAdapter.Object,
            backgroundJobScheduler.Object,
            clock);

        // Act
        await Assert.ThrowsAsync<Exception>(() => job.Execute(CancellationToken.None));

        // Assert
        var jobInfo = await dbContext.InternationalQtsAwardedEmailsJobs.SingleOrDefaultAsync(j => j.ExecutedUtc == today);
        Assert.Null(jobInfo);
        var jobItem = await dbContext.InternationalQtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.PersonId == qtsAwardee1.TeacherId);
        Assert.Null(jobItem);
    }
}
