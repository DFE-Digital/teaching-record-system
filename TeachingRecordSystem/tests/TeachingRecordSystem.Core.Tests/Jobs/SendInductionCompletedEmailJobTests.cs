using System.Text.Json;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendInductionCompletedEmailJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_WhenCalled_GetsTrnTokenSendsEmailAddsEventAndUpdatesDatabase()
    {
        // Arrange
        var notificationSender = new Mock<INotificationSender>();
        var getAnIdentityApiClient = new Mock<IGetAnIdentityApiClient>();
        var inductionCompletedEmailsJobId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var trn = "1234567";
        var emailAddress = Faker.Internet.Email();
        var firstName = Faker.Name.First();
        var lastName = Faker.Name.Last();
        var personalisation = new Dictionary<string, string>()
        {
            { "first name", firstName },
            { "last name", lastName }
        };

        await WithDbContextAsync(async dbContext =>
        {
            var batchJob = new InductionCompletedEmailsJob
            {
                InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
                PassedEndUtc = Clock.UtcNow.AddDays(-1),
                ExecutedUtc = Clock.UtcNow
            };
            dbContext.InductionCompletedEmailsJobs.Add(batchJob);

            var jobItem = new InductionCompletedEmailsJobItem
            {
                InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
                PersonId = personId,
                Trn = trn,
                EmailAddress = emailAddress,
                Personalization = personalisation
            };
            dbContext.InductionCompletedEmailsJobItems.Add(jobItem);
            await dbContext.SaveChangesAsync();
        });

        var tokenResponse = new CreateTrnTokenResponse
        {
            Email = emailAddress,
            Trn = trn,
            TrnToken = "Thisismytrntoken",
            ExpiresUtc = Clock.UtcNow.AddDays(60)
        };

        getAnIdentityApiClient
            .Setup(i => i.CreateTrnTokenAsync(
                It.Is<CreateTrnTokenRequest>(r => r.Trn == trn && r.Email == emailAddress)))
            .ReturnsAsync(tokenResponse);

        // Act
        await WithServiceAsync<SendInductionCompletedEmailJob>(
            job => job.ExecuteAsync(inductionCompletedEmailsJobId, personId),
            notificationSender.Object,
            getAnIdentityApiClient.Object);

        // Assert
        notificationSender
            .Verify(
                n => n.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s == emailAddress),
                    It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

        var updatedJobItem = await WithDbContextAsync(dbContext => dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(i =>
            i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.PersonId == personId));
        Assert.NotNull(updatedJobItem);
        Assert.True(updatedJobItem.EmailSent);

        var events = await WithDbContextAsync(dbContext => dbContext.Events
            .Where(e => e.EventName == "InductionCompletedEmailSentEvent")
            .ToListAsync());
        var emailSentEvent = events
            .Select(e => JsonSerializer.Deserialize<LegacyEvents.InductionCompletedEmailSentEvent>(e.Payload))
            .SingleOrDefault(e => e!.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && e.PersonId == personId);
        Assert.NotNull(emailSentEvent);
    }
}
