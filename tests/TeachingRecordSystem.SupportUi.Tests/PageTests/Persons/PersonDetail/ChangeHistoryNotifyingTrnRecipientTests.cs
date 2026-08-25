namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public partial class ChangeHistoryTests
{
    [Fact]
    public async Task Get_WithNotifyingTrnRecipientProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var email = new EventModels.Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = "template-123",
            EmailAddress = person.EmailAddress ?? "test@example.com",
            Personalization = new Dictionary<string, string>(),
            Metadata = new Dictionary<string, object>(),
            SentOn = TimeProvider.UtcNow,
            EmailReplyToId = null
        };

        var @event = new EmailSentEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Email = email
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.NotifyingTrnRecipient,
            userId: null,
            changeReason: null,
            @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            $"TRN email sent to {person.FirstName} {person.LastName}",
            "System",
            process.CreatedOn);
    }
}
