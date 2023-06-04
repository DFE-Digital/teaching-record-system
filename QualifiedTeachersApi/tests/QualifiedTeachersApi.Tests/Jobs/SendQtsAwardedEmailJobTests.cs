using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Events;
using QualifiedTeachersApi.Jobs;
using QualifiedTeachersApi.Services.AccessYourQualifications;
using QualifiedTeachersApi.Services.GetAnIdentity.Api.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[Collection("Job")]
public class SendQtsAwardedEmailJobTests : IAsyncLifetime
{
    private readonly JobFixture _jobFixture;

    public SendQtsAwardedEmailJobTests(JobFixture jobFixture)
    {
        _jobFixture = jobFixture;
    }

    public Task InitializeAsync() => _jobFixture.InitializeAsync();

    public Task DisposeAsync() => _jobFixture.DisposeAsync();

    [Fact]
    public async Task Execute_WhenCalled_GetsTrnTokenSendsEmailAddsEventAndUpdatesDatabase()
    {
        // Arrange
        var utcNow = new DateTime(2023, 02, 06, 08, 00, 00, DateTimeKind.Utc);
        using var dbContext = _jobFixture.DbFixture.GetDbContext();
        var accessYourQualificationOptions = Options.Create(
            new AccessYourQualificationsOptions
            {
                BaseAddress = "https://aytq.com"
            });
        _jobFixture.Clock.UtcNow = utcNow;
        var qtsAwardedEmailsJobId = Guid.NewGuid();
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

        var batchJob = new QtsAwardedEmailsJob
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            AwardedToUtc = utcNow.AddDays(-1),
            ExecutedUtc = utcNow
        };
        dbContext.QtsAwardedEmailsJobs.Add(batchJob);

        var jobItem = new QtsAwardedEmailsJobItem
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            PersonId = personId,
            Trn = trn,
            EmailAddress = emailAddress,
            Personalization = personalisation
        };
        dbContext.QtsAwardedEmailsJobItems.Add(jobItem);
        await dbContext.SaveChangesAsync();

        var tokenResponse = new CreateTrnTokenResponse
        {
            Email = emailAddress,
            Trn = trn,
            TrnToken = "Thisismytrntoken",
            ExpiresUtc = utcNow.AddDays(60)
        };

        _jobFixture.GetAnIdentityApiClient
            .Setup(i => i.CreateTrnToken(It.Is<CreateTrnTokenRequest>(r => r.Trn == trn && r.Email == emailAddress)))
            .ReturnsAsync(tokenResponse);

        var job = new SendQtsAwardedEmailJob(
            _jobFixture.NotificationSender.Object,
            dbContext,
            _jobFixture.GetAnIdentityApiClient.Object,
            accessYourQualificationOptions,
            _jobFixture.Clock);

        // Act
        await job.Execute(qtsAwardedEmailsJobId, personId);

        // Assert
        _jobFixture.NotificationSender
            .Verify(n => n.SendEmail(It.IsAny<string>(), It.Is<string>(s => s == emailAddress), It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

        var updatedJobItem = await dbContext.QtsAwardedEmailsJobItems.SingleOrDefaultAsync(i => i.QtsAwardedEmailsJobId == qtsAwardedEmailsJobId && i.PersonId == personId);
        Assert.NotNull(updatedJobItem);
        Assert.True(updatedJobItem.EmailSent);

        var events = await dbContext.Events
                                .Where(e => e.EventName == "QtsAwardedEmailSentEvent")
                                .ToListAsync();
        var emailSentEvent = events
                .Select(e => JsonSerializer.Deserialize<QtsAwardedEmailSentEvent>(e.Payload))
                .Where(e => e!.QtsAwardedEmailsJobId == qtsAwardedEmailsJobId && e.PersonId == personId)
                .SingleOrDefault();
        Assert.NotNull(emailSentEvent);
    }
}
