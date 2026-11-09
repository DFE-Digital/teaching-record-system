using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class SupportTaskZendeskUrlsUpdatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task AddedUrl_RendersAddedLine()
    {
        // Arrange
        const string addedUrl = "https://example.zendesk.com/agent/tickets/123";
        var process = await CreateProcessAsync([], [addedUrl]);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "Zendesk tickets changed");

        var message = entry.GetElementsByClassName("govuk-body").Single();
        Assert.Contains("added", message.TrimmedText());

        var link = message.QuerySelector("a") as IHtmlAnchorElement;
        Assert.NotNull(link);
        Assert.Equal(addedUrl, link!.GetAttribute("href"));
        Assert.Contains(addedUrl, link.TextContent);
    }

    [Fact]
    public async Task RemovedUrl_RendersRemovedLine()
    {
        // Arrange
        const string removedUrl = "https://example.zendesk.com/agent/tickets/123";
        var process = await CreateProcessAsync([removedUrl], []);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "Zendesk tickets changed");

        var message = entry.GetElementsByClassName("govuk-body").Single();
        Assert.Contains("removed", message.TrimmedText());

        var link = message.QuerySelector("a") as IHtmlAnchorElement;
        Assert.NotNull(link);
        Assert.Equal(removedUrl, link!.GetAttribute("href"));
        Assert.Contains(removedUrl, link.TextContent);
    }

    [Fact]
    public async Task ChangedSingleUrl_RendersRemovedThenAddedLines()
    {
        // Arrange
        const string oldUrl = "https://example.zendesk.com/agent/tickets/123";
        const string newUrl = "https://example.zendesk.com/agent/tickets/456";
        var process = await CreateProcessAsync([oldUrl], [newUrl]);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "Zendesk tickets changed");

        var messages = entry.GetElementsByClassName("govuk-body").Select(m => m.TrimmedText()).ToArray();
        Assert.Equal(2, messages.Length);
        Assert.Contains(oldUrl, messages[0]);
        Assert.Contains("removed", messages[0]);
        Assert.Contains(newUrl, messages[1]);
        Assert.Contains("added", messages[1]);
    }

    [Fact]
    public async Task ReorderedUrls_RendersDisplayOrderChanged()
    {
        // Arrange
        const string firstUrl = "https://example.zendesk.com/agent/tickets/123";
        const string secondUrl = "https://example.zendesk.com/agent/tickets/456";
        var process = await CreateProcessAsync([firstUrl, secondUrl], [secondUrl, firstUrl]);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "Zendesk tickets changed");
        var message = entry.GetElementsByClassName("govuk-body").Single();
        Assert.Equal("Display order changed", message.TrimmedText());
    }

    private Task<Core.DataStore.Postgres.Models.Process> CreateProcessAsync(string[] oldZendeskUrls, string[] zendeskUrls)
    {
        var oldSupportTask = new EventModels.SupportTask
        {
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.TrnRequest,
            Status = SupportTaskStatus.InProgress,
            OneLoginUserSubject = null,
            PersonId = null,
            Data = new TrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            ZendeskTickets = oldZendeskUrls,
            Outcome = null
        };

        var supportTask = oldSupportTask with
        {
            ZendeskTickets = zendeskUrls
        };

        return TestData.CreateProcessAsync(
            ProcessType.SupportTaskZendeskUrlsUpdating,
            changeReason: null,
            events: new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = "TEST-ST-1",
                Changes = SupportTaskUpdatedEventChanges.ZendeskTicketUrls,
                OldSupportTask = oldSupportTask,
                SupportTask = supportTask,
                Comments = null,
                RejectionReason = null
            });
    }
}
