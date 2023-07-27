﻿using TeachingRecordSystem.Api.Jobs;
using TeachingRecordSystem.Api.Jobs.Scheduling;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.Tests.Jobs;

public class EytsAwardedEmailJobDispatcherTests : EytsAwardedEmailJobTestBase
{
    public EytsAwardedEmailJobDispatcherTests(DbFixture dbFixture)
        : base(dbFixture)
    {
    }

    [Test]
    public async Task Execute_WhenCalled_EnqueuesSendEmailJobForAllUnsentItems()
    {
        // Arrange
        var utcNow = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = DbFixture.GetDbContext();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();
        var eytsAwardedEmailsJobId = Guid.NewGuid();
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

        var batchJob = new EytsAwardedEmailsJob
        {
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            AwardedToUtc = utcNow.AddDays(-1),
            ExecutedUtc = utcNow
        };
        dbContext.EytsAwardedEmailsJobs.Add(batchJob);

        var jobItem1 = new EytsAwardedEmailsJobItem
        {
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            PersonId = teacher1PersonId,
            Trn = teacher1Trn,
            EmailAddress = teacher1EmailAddress,
            Personalization = teacher1Personalisation,
            EmailSent = false
        };
        dbContext.EytsAwardedEmailsJobItems.Add(jobItem1);
        var jobItem2 = new EytsAwardedEmailsJobItem
        {
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            PersonId = teacher2PersonId,
            Trn = teacher2Trn,
            EmailAddress = teacher2EmailAddress,
            Personalization = teacher2Personalisation,
            EmailSent = true
        };
        dbContext.EytsAwardedEmailsJobItems.Add(jobItem2);
        var jobItem3 = new EytsAwardedEmailsJobItem
        {
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            PersonId = teacher3PersonId,
            Trn = teacher3Trn,
            EmailAddress = teacher3EmailAddress,
            Personalization = teacher3Personalisation,
            EmailSent = false
        };
        dbContext.EytsAwardedEmailsJobItems.Add(jobItem3);
        await dbContext.SaveChangesAsync();

        var dispatcher = new EytsAwardedEmailJobDispatcher(
            dbContext,
            backgroundJobScheduler.Object);

        // Act
        await dispatcher.Execute(eytsAwardedEmailsJobId);

        // Assert
        backgroundJobScheduler
            .Verify(s => s.Enqueue(It.IsAny<System.Linq.Expressions.Expression<Func<SendEytsAwardedEmailJob, Task>>>()), Times.Exactly(2));
    }
}
