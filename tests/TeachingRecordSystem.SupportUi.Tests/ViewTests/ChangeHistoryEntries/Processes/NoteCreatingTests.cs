using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class NoteCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var noteContent = "Test note content";

        // Act
        var entry = await PublishNoteCreatedEventAsync(person.PersonId, user.UserId, noteContent);

        // Assert
        AssertTitle(entry, "Notes added");

        var messageText = entry.QuerySelector(".govuk-body")?.TextContent?.Trim();
        Assert.Equal($"Note added for {person.FirstName} {person.LastName}.", messageText);

        var details = entry.QuerySelector("[data-testid='note-content']");
        Assert.NotNull(details);

        var detailsSummary = details.QuerySelector(".govuk-details__summary-text")?.TextContent?.Trim();
        Assert.Equal("Note", detailsSummary);

        var detailsText = details.QuerySelector(".govuk-details__text")?.TextContent?.Trim();
        Assert.Equal(noteContent, detailsText);
    }

    private async Task<IHtmlElement> PublishNoteCreatedEventAsync(
        Guid personId,
        Guid userId,
        string noteContent = "Test note content")
    {
        var noteId = Guid.NewGuid();

        var process = await TestData.CreateProcessAsync(
            ProcessType.NoteCreating,
            userId,
            changeReason: null,
            new NoteCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Note = new EventModels.Note
                {
                    NoteId = noteId,
                    PersonId = personId,
                    Content = noteContent,
                    File = null
                }
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
