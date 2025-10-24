using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendAytqInviteEmailJobTests(CoreFixture fixture) : NightlyEmailJobTestBase(fixture)
{
    [Fact]
    public Task Execute_WhenCalled_GetsTrnTokenSendsEmailAddsEventAndUpdatesDatabase() =>
        DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var notificationSender = new Mock<INotificationSender>();
            var getAnIdentityApiClient = new Mock<IGetAnIdentityApiClient>();

            var accessYourQualificationOptions = Options.Create(
                new AccessYourTeachingQualificationsOptions
                {
                    BaseAddress = "https://aytq.com"
                });

            var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));

            var templateId = Guid.NewGuid().ToString();

            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = templateId,
                EmailAddress = person.EmailAddress!,
                Personalization = new Dictionary<string, string>
                {
                    ["first name"] = person.FirstName,
                    ["last name"] = person.LastName
                },
                Metadata = new Dictionary<string, object> { ["Trn"] = person.Trn! },
                SentOn = null
            };

            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();

            var tokenResponse = new CreateTrnTokenResponse
            {
                Email = person.EmailAddress!,
                Trn = person.Trn!,
                TrnToken = "ThisIsMyTrnToken",
                ExpiresUtc = Clock.UtcNow.AddDays(60)
            };

            getAnIdentityApiClient
                .Setup(i => i.CreateTrnTokenAsync(
                    It.Is<CreateTrnTokenRequest>(r => r.Trn == tokenResponse.Trn && r.Email == tokenResponse.Email)))
                .ReturnsAsync(tokenResponse);

            var job = new SendAytqInviteEmailJob(
                notificationSender.Object,
                dbContext,
                getAnIdentityApiClient.Object,
                accessYourQualificationOptions,
                Clock);

            // Act
            await job.ExecuteAsync(email.EmailId);

            // Assert
            notificationSender
                .Verify(
                    n => n.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s == person.EmailAddress),
                        It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

            Assert.Equal(Clock.UtcNow, email.SentOn);

            var events = await dbContext.Events
                .Where(e => e.EventName == nameof(EmailSentEvent))
                .ToListAsync();

            var @event = Assert.Single(events);
            var emailSentEvent = (EmailSentEvent)@event.ToEventBase();

            Assert.NotNull(emailSentEvent);
        });
}
