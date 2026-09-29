namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class ChangeOfNameRequestCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var requestedFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var requestedMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName ?? string.Empty);
        var requestedLastName = TestData.GenerateChangedLastName(person.LastName);

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithFirstName(requestedFirstName)
                .WithMiddleName(requestedMiddleName)
                .WithLastName(requestedLastName)
                .WithEvidenceFileId(Guid.NewGuid())
                .WithEvidenceFileName("evidence.pdf")
                .WithoutEmailAddress());

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestCreating,
            user.UserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(dbSupportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Change of name request submitted");
        var requestedName = string.JoinNonEmpty(' ', requestedFirstName, requestedMiddleName, requestedLastName);
        Assert.Equal(
            $"Change of name request submitted to change to {requestedName}.",
            entry.GetElementByTestId("change-request-context")?.TrimmedText());

        var supportTaskLink = entry.GetElementByTestId("support-task-link");
        Assert.NotNull(supportTaskLink);
        Assert.Equal("Change of name request", supportTaskLink?.TrimmedText());
        Assert.Equal($"/support-tasks/{dbSupportTask.SupportTaskReference}", supportTaskLink?.GetAttribute("href"));

        Assert.Null(entry.GetElementByTestId("person-link"));

        var evidenceLink = entry.GetElementByTestId("uploaded-evidence-file-link");
        Assert.NotNull(evidenceLink);
        Assert.Contains("evidence.pdf", evidenceLink?.TextContent);
    }

    [Fact]
    public async Task ProcessRendersCorrectly_WithSupportTaskContext()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var requestedFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var requestedMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName ?? string.Empty);
        var requestedLastName = TestData.GenerateChangedLastName(person.LastName);

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithFirstName(requestedFirstName)
                .WithMiddleName(requestedMiddleName)
                .WithLastName(requestedLastName)
                .WithEvidenceFileId(Guid.NewGuid())
                .WithEvidenceFileName("evidence.pdf")
                .WithoutEmailAddress());

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestCreating,
            user.UserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(dbSupportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: dbSupportTask.SupportTaskReference);

        // Assert
        AssertTitle(entry, "Change of name request submitted");
        var requestedName = string.JoinNonEmpty(' ', requestedFirstName, requestedMiddleName, requestedLastName);
        Assert.Equal(
            $"Change of name request submitted to change to {requestedName} for TRN {person.Trn}.",
            entry.GetElementByTestId("change-request-context")?.TrimmedText());

        var personLink = entry.GetElementByTestId("person-link");
        Assert.NotNull(personLink);
        Assert.Equal(person.Trn, personLink?.TrimmedText());
        Assert.Equal($"/persons/{person.PersonId}", personLink?.GetAttribute("href"));

        Assert.Null(entry.GetElementByTestId("support-task-link"));

        var evidenceLink = entry.GetElementByTestId("uploaded-evidence-file-link");
        Assert.NotNull(evidenceLink);
        Assert.Contains("evidence.pdf", evidenceLink?.TextContent);
    }
}
