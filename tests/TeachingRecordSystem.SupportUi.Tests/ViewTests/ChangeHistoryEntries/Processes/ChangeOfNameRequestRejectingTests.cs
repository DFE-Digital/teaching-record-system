namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class ChangeOfNameRequestRejectingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithFirstName(TestData.GenerateChangedFirstName(person.FirstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(person.MiddleName ?? string.Empty))
                .WithLastName(TestData.GenerateChangedLastName(person.LastName))
                .WithEvidenceFileId(Guid.NewGuid())
                .WithEvidenceFileName("evidence.pdf")
                .WithoutEmailAddress());

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeNameRequest_Rejected
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestRejecting,
            user.UserId,
            changeReason: null,
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status,
                SupportTask = supportTask,
                OldSupportTask = oldSupportTask,
                Comments = null,
                RejectionReason = "Insufficient evidence"
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Change of Name request rejected");
        Assert.Equal(
            "Request to change name rejected.",
            entry.GetElementByTestId("change-request-context")?.TrimmedText());

        var supportTaskLink = entry.GetElementByTestId("support-task-link");
        Assert.NotNull(supportTaskLink);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", supportTaskLink?.GetAttribute("href"));
        Assert.Equal("Task", supportTaskLink?.TrimmedText());
    }
}
