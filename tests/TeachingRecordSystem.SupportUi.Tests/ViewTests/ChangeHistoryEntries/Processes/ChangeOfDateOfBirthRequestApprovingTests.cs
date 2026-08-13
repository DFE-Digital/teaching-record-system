namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class ChangeOfDateOfBirthRequestApprovingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var supportTaskRequestData = new Core.Models.SupportTasks.ChangeDateOfBirthRequestData
        {
            ChangeRequestOutcome = null,
            EmailAddress = null,
            EvidenceFileId = Guid.NewGuid(),
            EvidenceFileName = "evidence.pdf",
            DateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value)
        };

        var dbSupportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithDateOfBirth(supportTaskRequestData.DateOfBirth)
                .WithEvidenceFileId(supportTaskRequestData.EvidenceFileId)
                .WithEvidenceFileName(supportTaskRequestData.EvidenceFileName)
                .WithoutEmailAddress());

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeDateOfBirthRequest_Approved
        };

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
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                DateOfBirth = supportTaskRequestData.DateOfBirth,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            OldPersonDetails = new EventModels.PersonDetails
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            Changes = PersonDetailsUpdatedEventChanges.DateOfBirth
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfDateOfBirthRequestApproving,
            user.UserId,
            changeReason: null,
            supportTaskUpdatedEvent,
            personDetailsUpdatedEvent);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Change of date of birth request accepted");
        Assert.Equal(
            $"Date of birth changed from {person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat)} to {supportTaskRequestData.DateOfBirth.ToString(WebConstants.DateDisplayFormat)}.",
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
