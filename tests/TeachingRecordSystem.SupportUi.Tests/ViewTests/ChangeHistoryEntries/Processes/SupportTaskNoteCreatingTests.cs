using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class SupportTaskNoteCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var noteContent = "Test note content";

        // Act
        var entry = await PublishSupportTaskNoteCreatedEventAsync(user.UserId, noteContent);

        // Assert
        AssertTitle(entry, "Noted added");

        var details = entry.QuerySelector("[data-testid='note-content']");
        Assert.NotNull(details);

        var detailsSummary = details.QuerySelector(".govuk-details__summary-text")?.TextContent?.Trim();
        Assert.Equal("Note", detailsSummary);

        var detailsText = details.QuerySelector(".govuk-details__text")?.TextContent?.Trim();
        Assert.Equal(noteContent, detailsText);
    }

    private async Task<IHtmlElement> PublishSupportTaskNoteCreatedEventAsync(
        Guid userId,
        string noteContent = "Test note content")
    {
        var supportTaskReference = "TEST-ST-1";

        var process = await TestData.CreateProcessAsync(
            ProcessType.SupportTaskNoteCreating,
            userId,
            changeReason: null,
            new SupportTaskNoteCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskNote = new EventModels.SupportTaskNote
                {
                    SupportTaskNoteId = Guid.NewGuid(),
                    SupportTaskReference = supportTaskReference,
                    Content = noteContent
                }
            });

        return await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: supportTaskReference);
    }
}
