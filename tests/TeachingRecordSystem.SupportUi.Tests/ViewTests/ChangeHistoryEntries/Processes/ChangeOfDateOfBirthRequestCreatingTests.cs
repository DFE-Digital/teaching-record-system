namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class ChangeOfDateOfBirthRequestCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var requestedDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value);

        var dbSupportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithDateOfBirth(requestedDateOfBirth)
                .WithEvidenceFileId(Guid.NewGuid())
                .WithEvidenceFileName("evidence.pdf")
                .WithoutEmailAddress());

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfDateOfBirthRequestCreating,
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
        AssertTitle(entry, "Change of date of birth request created");
        Assert.Equal(
            $"Request to change date of birth to {requestedDateOfBirth.ToString(WebConstants.DateDisplayFormat)}.",
            entry.GetElementByTestId("change-request-context")?.TrimmedText());
        Assert.Null(entry.GetElementByTestId("support-task-link"));
    }
}
