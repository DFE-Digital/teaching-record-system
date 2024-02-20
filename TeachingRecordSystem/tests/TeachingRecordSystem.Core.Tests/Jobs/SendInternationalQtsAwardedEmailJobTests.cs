using System.Text.Json;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendInternationalQtsAwardedEmailJobTests : InternationalQtsAwardedEmailJobTestBase
{
    public SendInternationalQtsAwardedEmailJobTests(DbFixture dbFixture)
        : base(dbFixture)
    {
    }

    [Fact]
    public async Task Execute_WhenCalled_GetsTrnTokenSendsEmailAddsEventAndUpdatesDatabase()
    {
        // Arrange
        var utcNow = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = DbFixture.GetDbContext();
        var clock = new TestableClock();
        var notificationSender = new Mock<INotificationSender>();
        var getAnIdentityApiClient = new Mock<IGetAnIdentityApiClient>();
        var accessYourQualificationOptions = Options.Create(
            new AccessYourTeachingQualificationsOptions
            {
                BaseAddress = "https://aytq.com"
            });
        clock.UtcNow = utcNow;
        var internationalQtsAwardedEmailsJobId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var trn = "1234567";
        var emailAddress = Faker.Internet.Email();
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var personalisation = new Dictionary<string, string>()
        {
            { "first name", firstName },
            { "last name", lastName },
        };

        var batchJob = new InternationalQtsAwardedEmailsJob
        {
            InternationalQtsAwardedEmailsJobId = internationalQtsAwardedEmailsJobId,
            AwardedToUtc = utcNow.AddDays(-1),
            ExecutedUtc = utcNow
        };
        dbContext.InternationalQtsAwardedEmailsJobs.Add(batchJob);

        var jobItem = new InternationalQtsAwardedEmailsJobItem
        {
            InternationalQtsAwardedEmailsJobId = internationalQtsAwardedEmailsJobId,
            PersonId = personId,
            Trn = trn,
            EmailAddress = emailAddress,
            Personalization = personalisation
        };
        dbContext.InternationalQtsAwardedEmailsJobItems.Add(jobItem);
        await dbContext.SaveChangesAsync();

        var tokenResponse = new CreateTrnTokenResponse
        {
            Email = emailAddress,
            Trn = trn,
            TrnToken = "Thisismytrntoken",
            ExpiresUtc = utcNow.AddDays(60)
        };

        getAnIdentityApiClient
            .Setup(i => i.CreateTrnToken(It.Is<CreateTrnTokenRequest>(r => r.Trn == trn && r.Email == emailAddress)))
            .ReturnsAsync(tokenResponse);

        var job = new SendInternationalQtsAwardedEmailJob(
            notificationSender.Object,
            dbContext,
            getAnIdentityApiClient.Object,
            accessYourQualificationOptions,
            clock);

        // Act
        await job.Execute(internationalQtsAwardedEmailsJobId, personId);

        // Assert
        notificationSender
            .Verify(n => n.SendEmail(It.IsAny<string>(), It.Is<string>(s => s == emailAddress), It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

        var updatedJobItem = await dbContext.InternationalQtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.InternationalQtsAwardedEmailsJobId == internationalQtsAwardedEmailsJobId && i.PersonId == personId);
        Assert.NotNull(updatedJobItem);
        Assert.True(updatedJobItem.EmailSent);

        var events = await dbContext.Events
                                .Where(e => e.EventName == "InternationalQtsAwardedEmailSentEvent")
                                .ToListAsync();
        var emailSentEvent = events
                .Select(e => JsonSerializer.Deserialize<InternationalQtsAwardedEmailSentEvent>(e.Payload))
                .Where(e => e!.InternationalQtsAwardedEmailsJobId == internationalQtsAwardedEmailsJobId && e.PersonId == personId)
                .SingleOrDefault();
        Assert.NotNull(emailSentEvent);
    }
}
