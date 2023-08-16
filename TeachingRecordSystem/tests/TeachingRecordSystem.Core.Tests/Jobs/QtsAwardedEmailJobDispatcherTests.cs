using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class QtsAwardedEmailJobDispatcherTests : QtsAwardedEmailJobTestBase
{
    public QtsAwardedEmailJobDispatcherTests(DbFixture dbFixture)
        : base(dbFixture)
    {
    }

    [Fact]
    public async Task Execute_WhenCalled_EnqueuesSendEmailJobForAllUnsentItems()
    {
        // Arrange
        var utcNow = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = DbFixture.GetDbContext();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var qtsAwardedEmailsJobId = Guid.NewGuid();
        var teacher1PersonId = Guid.NewGuid();
        var teacher1Trn = "1234567";
        var teacher1EmailAddress = Faker.Internet.Email();
        var teacher1FirstName = Faker.Name.First();
        var teacher1LastName = Faker.Name.Last();
        var teacher1Personalisation = new Dictionary<string, string>()
        {
            { "first name", teacher1FirstName },
            { "last name", teacher1LastName },
        };
        var teacher2PersonId = Guid.NewGuid();
        var teacher2Trn = "1234568";
        var teacher2EmailAddress = Faker.Internet.Email();
        var teacher2FirstName = Faker.Name.First();
        var teacher2LastName = Faker.Name.Last();
        var teacher2Personalisation = new Dictionary<string, string>()
        {
            { "first name", teacher1FirstName },
            { "last name", teacher1LastName },
        };
        var teacher3PersonId = Guid.NewGuid();
        var teacher3Trn = "1234569";
        var teacher3EmailAddress = Faker.Internet.Email();
        var teacher3FirstName = Faker.Name.First();
        var teacher3LastName = Faker.Name.Last();
        var teacher3Personalisation = new Dictionary<string, string>()
        {
            { "first name", teacher1FirstName },
            { "last name", teacher1LastName },
        };

        var batchJob = new QtsAwardedEmailsJob
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            AwardedToUtc = utcNow.AddDays(-1),
            ExecutedUtc = utcNow
        };
        dbContext.QtsAwardedEmailsJobs.Add(batchJob);

        var jobItem1 = new QtsAwardedEmailsJobItem
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            PersonId = teacher1PersonId,
            Trn = teacher1Trn,
            EmailAddress = teacher1EmailAddress,
            Personalization = teacher1Personalisation,
            EmailSent = false
        };
        dbContext.QtsAwardedEmailsJobItems.Add(jobItem1);
        var jobItem2 = new QtsAwardedEmailsJobItem
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            PersonId = teacher2PersonId,
            Trn = teacher2Trn,
            EmailAddress = teacher2EmailAddress,
            Personalization = teacher2Personalisation,
            EmailSent = true
        };
        dbContext.QtsAwardedEmailsJobItems.Add(jobItem2);
        var jobItem3 = new QtsAwardedEmailsJobItem
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            PersonId = teacher3PersonId,
            Trn = teacher3Trn,
            EmailAddress = teacher3EmailAddress,
            Personalization = teacher3Personalisation,
            EmailSent = false
        };
        dbContext.QtsAwardedEmailsJobItems.Add(jobItem3);
        await dbContext.SaveChangesAsync();

        var dispatcher = new QtsAwardedEmailJobDispatcher(
            dbContext,
            backgroundJobScheduler.Object);

        // Act
        await dispatcher.Execute(qtsAwardedEmailsJobId);

        // Assert
        backgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<SendQtsAwardedEmailJob, Task>>>()), Times.Exactly(2));
    }
}
