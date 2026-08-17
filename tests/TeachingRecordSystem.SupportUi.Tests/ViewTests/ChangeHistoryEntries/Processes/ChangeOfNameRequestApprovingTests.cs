namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class ChangeOfNameRequestApprovingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var supportTaskRequestData = new Core.Models.SupportTasks.ChangeNameRequestData
        {
            ChangeRequestOutcome = null,
            EmailAddress = null,
            EvidenceFileId = Guid.NewGuid(),
            EvidenceFileName = "evidence.pdf",
            FirstName = TestData.GenerateChangedFirstName(person.FirstName),
            MiddleName = TestData.GenerateChangedMiddleName(person.MiddleName ?? string.Empty),
            LastName = TestData.GenerateChangedLastName(person.LastName)
        };

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithFirstName(supportTaskRequestData.FirstName!)
                .WithMiddleName(supportTaskRequestData.MiddleName!)
                .WithLastName(supportTaskRequestData.LastName!)
                .WithEvidenceFileId(supportTaskRequestData.EvidenceFileId)
                .WithEvidenceFileName(supportTaskRequestData.EvidenceFileName)
                .WithoutEmailAddress());

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeNameRequest_Approved
        };

        var oldName = string.JoinNonEmpty(' ', [person.FirstName, person.MiddleName, person.LastName]);
        var newName = string.JoinNonEmpty(' ', [supportTaskRequestData.FirstName, supportTaskRequestData.MiddleName, supportTaskRequestData.LastName]);

        var supportTaskUpdatedEvent = new SupportTaskUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTaskReference = supportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status,
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            Comments = null,
            RejectionReason = null
        };

        var personDetailsUpdatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            PersonDetails = new EventModels.PersonDetails
            {
                FirstName = supportTaskRequestData.FirstName!,
                MiddleName = supportTaskRequestData.MiddleName,
                LastName = supportTaskRequestData.LastName,
                DateOfBirth = null,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            OldPersonDetails = new EventModels.PersonDetails
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName!,
                LastName = person.LastName,
                DateOfBirth = null,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            Changes = PersonDetailsUpdatedEventChanges.NameChange
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestApproving,
            user.UserId,
            changeReason: null,
            supportTaskUpdatedEvent,
            personDetailsUpdatedEvent);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Change of Name request accepted");
        Assert.Equal(
            $"Name changed from {oldName} to {newName}.",
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
