namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogChangeNameOrDobRequestProcessTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Person_WithChangeOfNameRequestApprovingProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithFirstName(newFirstName)
                .WithMiddleName(newMiddleName)
                .WithLastName(newLastName));

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeNameRequest_Approved
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestApproving,
            user.UserId,
            changeReason: null,
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                SupportTask = supportTask,
                OldSupportTask = oldSupportTask,
                Comments = null,
                RejectionReason = null
            },
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                PersonDetails = new EventModels.PersonDetails
                {
                    FirstName = newFirstName,
                    MiddleName = newMiddleName,
                    LastName = newLastName,
                    DateOfBirth = person.DateOfBirth,
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
                Changes = PersonDetailsUpdatedEventChanges.NameChange
            });

        var oldName = string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
        var newName = string.JoinNonEmpty(' ', newFirstName, newMiddleName, newLastName);

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Change of Name request accepted",
            user.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        Assert.Equal($"Name changed from {oldName} to {newName}.", item.GetElementByTestId("change-request-context")?.TrimmedText());

        var supportTaskLink = item.GetElementByTestId("support-task-link");
        Assert.NotNull(supportTaskLink);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", supportTaskLink.GetAttribute("href"));
    }

    [Fact]
    public async Task Person_WithChangeOfDateOfBirthRequestApprovingProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value);

        var dbSupportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(newDateOfBirth));

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeDateOfBirthRequest_Approved
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.ChangeOfDateOfBirthRequestApproving,
            user.UserId,
            changeReason: null,
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                SupportTask = supportTask,
                OldSupportTask = oldSupportTask,
                Comments = null,
                RejectionReason = null
            },
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                PersonDetails = new EventModels.PersonDetails
                {
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    DateOfBirth = newDateOfBirth,
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
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Change of date of birth request accepted",
            user.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        Assert.Equal(
            $"Date of birth changed from {person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat)} to {newDateOfBirth.ToString(WebConstants.DateDisplayFormat)}.",
            item.GetElementByTestId("change-request-context")?.TrimmedText());

        var supportTaskLink = item.GetElementByTestId("support-task-link");
        Assert.NotNull(supportTaskLink);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", supportTaskLink.GetAttribute("href"));
    }
}
