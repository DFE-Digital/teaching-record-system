using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class NotifyingTrnRecipientTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var entry = await PublishEmailSentEventAsync(person);

        // Assert
        AssertTitle(entry, $"TRN email sent to {person.FirstName} {person.LastName}");

        var emailSentMessage = entry.GetElementByTestId("email-sent-message");
        Assert.NotNull(emailSentMessage);
        Assert.Equal("We’ve sent them an email confirming their TRN.", emailSentMessage!.TrimmedText());
    }

    private async Task<IHtmlElement> PublishEmailSentEventAsync(Person person)
    {
        var email = new EventModels.Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = "template-123",
            EmailAddress = Faker.Internet.Email(),
            Personalization = new Dictionary<string, string>(),
            Metadata = new Dictionary<string, object>(),
            SentOn = TimeProvider.UtcNow,
            EmailReplyToId = null
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.NotifyingTrnRecipient,
            changeReason: null,
            events: new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Email = email
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
