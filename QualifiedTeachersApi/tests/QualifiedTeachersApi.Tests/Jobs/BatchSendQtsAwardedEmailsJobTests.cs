using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Jobs;
using QualifiedTeachersApi.Jobs.Scheduling;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[Collection("Job")]
public class BatchSendQtsAwardedEmailsJobTests : IAsyncLifetime
{
    private readonly JobFixture _jobFixture;

    public BatchSendQtsAwardedEmailsJobTests(JobFixture jobFixture)
    {
        _jobFixture = jobFixture;
    }

    public static TheoryData<DateTime, DateTime?, DateTime, DateTime, DateTime> DateRangeEvaulationTestData { get; } = new()
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

    public Task InitializeAsync() => _jobFixture.InitializeAsync();

    public Task DisposeAsync() => _jobFixture.DisposeAsync();

    [Theory]
    [MemberData(nameof(DateRangeEvaulationTestData))]
    public async Task Execute_ForMultipleScenarios_EvaluatesDateRangeCorrectly(
        DateTime initialLastAwardedToUtc,
        DateTime? previousJobLastAwardedToUtc,
        DateTime utcNow,
        DateTime startExpected,
        DateTime endExpected)
    {
        // Arrange
        using var dbContext = _jobFixture.DbFixture.GetDbContext();
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

        var logger = new Mock<ILogger<BatchSendQtsAwardedEmailsJob>>();
        var jobOptions = Options.Create(
            new BatchSendQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        _jobFixture.Clock.UtcNow = utcNow;

        DateTime startActual = DateTime.MinValue;
        DateTime endActual = DateTime.MaxValue;
        _jobFixture.DataverseAdapter
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
            _jobFixture.DataverseAdapter.Object,
            _jobFixture.BackgroundJobScheduler.Object,
            _jobFixture.Clock,
            logger.Object);

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
        using var dbContext = _jobFixture.DbFixture.GetDbContext();
        var logger = new Mock<ILogger<BatchSendQtsAwardedEmailsJob>>();
        var jobOptions = Options.Create(
            new BatchSendQtsAwardedEmailsJobOptions
            {
                EmailDelayDays = 3,
                InitialLastAwardedToUtc = initialLastAwardedToUtc,
                JobSchedule = "0 8 * * *"
            });

        _jobFixture.Clock.UtcNow = today;

        var qtsAwardee1 = new QtsAwardee
        {
            TeacherId = Guid.NewGuid(),
            Trn = "1234567",
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            EmailAddress = Faker.Internet.Email()
        };

        var qtsAwardees = new[] { qtsAwardee1 };

        _jobFixture.DataverseAdapter
            .Setup(d => d.GetQtsAwardeesForDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(qtsAwardees);

        var job = new BatchSendQtsAwardedEmailsJob(
            jobOptions,
            dbContext,
            _jobFixture.DataverseAdapter.Object,
            _jobFixture.BackgroundJobScheduler.Object,
            _jobFixture.Clock,
            logger.Object);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var jobItem = await dbContext.QtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.PersonId == qtsAwardee1.TeacherId);
        Assert.NotNull(jobItem);
        Assert.Equal(qtsAwardee1.Trn, jobItem.Trn);
        Assert.Equal(qtsAwardee1.EmailAddress, jobItem.EmailAddress);
        Assert.Equal(qtsAwardee1.FirstName, jobItem.Personalization["first name"]);
        Assert.Equal(qtsAwardee1.LastName, jobItem.Personalization["last name"]);

        _jobFixture.BackgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<SendQtsAwardedEmailJob, Task>>>()), Times.Once);
    }
}
