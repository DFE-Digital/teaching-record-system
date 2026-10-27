using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class NotifyingProfessionalStatusRecipientTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Theory]
    [InlineData(ProcessType.NotifyingQtsAwardee, "QTS awarded", "We’ve sent them an email confirming their QTS award")]
    [InlineData(ProcessType.NotifyingInternationalQtsAwardee, "International QTS awarded", "We’ve sent them an email confirming their IQTS award.")]
    [InlineData(ProcessType.NotifyingEytsAwardee, "EYTS awarded", "We’ve sent them an email confirming their EYTS award.")]
    [InlineData(ProcessType.NotifyingQtlsAwardee, "QTLS awarded", "We’ve sent them an email confirming their QTLS award.")]
    [InlineData(ProcessType.NotifyingInductionCompletee, "Induction complete", "We’ve sent them an email confirming their induction is complete.")]
    [InlineData(ProcessType.NotifyingLapsedQtlsHolder, "QTLS revoked", "We’ve sent them an email confirming they no longer have QTLS")]
    public async Task ProcessRendersCorrectly(ProcessType processType, string expectedTitle, string expectedMessage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var entry = await PublishEmailSentEventAsync(person, processType);

        // Assert
        AssertTitle(entry, expectedTitle);

        var emailSentMessage = entry.GetElementByTestId("email-sent-message");
        Assert.NotNull(emailSentMessage);
        Assert.Equal(expectedMessage, emailSentMessage!.TrimmedText());
    }

    private async Task<IHtmlElement> PublishEmailSentEventAsync(Person person, ProcessType processType)
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
            processType,
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
