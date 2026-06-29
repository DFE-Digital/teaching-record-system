using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Notify;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SendAytqInviteEmailJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_WhenCalled_GetsTrnTokenSendsEmailAddsEventAndUpdatesDatabase()
    {
        // Arrange
        var notificationSender = new Mock<INotificationSender>();

        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));

        var templateId = Guid.NewGuid().ToString();

        var email = await WithDbContextAsync(async dbContext =>
        {
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
                Metadata = new Dictionary<string, object> { ["Trn"] = person.Trn },
                SentOn = null
            };

            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();
            return email;
        });

        // Act
        await WithServiceAsync<SendAytqInviteEmailJob>(job => job.ExecuteAsync(email.EmailId), notificationSender.Object);

        // Assert
        notificationSender
            .Verify(
                n => n.SendEmailAsync(It.IsAny<string>(), It.Is<string>(s => s == person.EmailAddress),
                    It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);

        var updatedEmail = await WithDbContextAsync(dbContext => dbContext.Emails.SingleAsync(e => e.EmailId == email.EmailId));
        Assert.Equal(TimeProvider.UtcNow, updatedEmail.SentOn);

        var events = await WithDbContextAsync(dbContext => dbContext.Events
            .Where(e => e.EventName == nameof(LegacyEvents.EmailSentEvent))
            .ToListAsync());

        var @event = Assert.Single(events);
        var emailSentEvent = (LegacyEvents.EmailSentEvent)@event.ToEventBase();

        Assert.NotNull(emailSentEvent);
    }
}
