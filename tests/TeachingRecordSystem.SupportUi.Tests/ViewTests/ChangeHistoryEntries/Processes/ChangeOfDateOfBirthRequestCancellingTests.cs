namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class ChangeOfDateOfBirthRequestCancellingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var dbSupportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value))
                .WithEvidenceFileId(Guid.NewGuid())
                .WithEvidenceFileName("evidence.pdf")
                .WithoutEmailAddress());

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeDateOfBirthRequest_Cancelled
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfDateOfBirthRequestCancelling,
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
                RejectionReason = null
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Change of date of birth request cancelled");
        Assert.Equal(
            "Request to change date of birth cancelled as no longer required.",
            entry.GetElementByTestId("change-request-context")?.TrimmedText());

        var supportTaskLink = entry.GetElementByTestId("support-task-link");
        Assert.NotNull(supportTaskLink);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", supportTaskLink?.GetAttribute("href"));
        Assert.Equal("Task", supportTaskLink?.TrimmedText());
        Assert.Equal(
            "Task closed.",
            string.Join(
                " ",
                entry.GetElementByTestId("support-task-closed-message")?.TextContent.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries) ?? []));
    }
}
