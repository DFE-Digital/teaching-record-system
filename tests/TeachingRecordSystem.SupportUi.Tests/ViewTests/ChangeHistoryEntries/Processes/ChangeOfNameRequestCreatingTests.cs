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
        AssertTitle(entry, "Change of Name request created");
        Assert.Equal(
            $"Request to change name to {string.JoinNonEmpty(' ', requestedFirstName, requestedMiddleName, requestedLastName)}.",
            entry.GetElementByTestId("change-request-context")?.TrimmedText());
        Assert.Null(entry.GetElementByTestId("support-task-link"));
    }
}
