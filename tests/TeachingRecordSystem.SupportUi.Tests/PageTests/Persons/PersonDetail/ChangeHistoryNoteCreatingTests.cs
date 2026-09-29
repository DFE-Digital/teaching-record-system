namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public partial class ChangeHistoryTests
{
    [Fact]
    public async Task Get_WithNoteCreatingProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var noteContent = "Test note content";

        var noteId = Guid.NewGuid();
        var @event = new NoteCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Note = new EventModels.Note
            {
                NoteId = noteId,
                PersonId = person.PersonId,
                Content = noteContent,
                File = null
            }
        };

        var process = await TestData.CreateProcessAsync(ProcessType.NoteCreating, user.UserId, changeReason: null, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var entry = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(entry);

        var title = entry.QuerySelector(".moj-timeline__title")?.TextContent;
        Assert.Equal("Notes added", title);

        var messageText = entry.QuerySelector(".govuk-body")?.TextContent?.Trim();
        Assert.Equal($"Note added for {person.FirstName} {person.LastName}.", messageText);

        var details = entry.QuerySelector("[data-testid='note-content']");
        Assert.NotNull(details);

        var detailsSummary = details.QuerySelector(".govuk-details__summary-text")?.TextContent?.Trim();
        Assert.Equal("Note", detailsSummary);

        var detailsText = details.QuerySelector(".govuk-details__text")?.TextContent?.Trim();
        Assert.Equal(noteContent, detailsText);
    }
}
