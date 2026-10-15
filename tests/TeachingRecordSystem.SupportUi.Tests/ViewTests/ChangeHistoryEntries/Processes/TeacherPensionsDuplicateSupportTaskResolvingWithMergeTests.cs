using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class TeacherPensionsDuplicateSupportTaskResolvingWithMergeTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectlyForRetainedPerson()
    {
        // Arrange
        var (retainedPerson, deactivatedPerson, supportTask, process) = await CreateProcessAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, retainedPerson.PersonId);

        // Assert
        AssertTitle(entry, "Teachers’ Pensions potential duplicate task resolved");

        var mergeMessage = entry.GetElementByTestId("merge-message");
        Assert.NotNull(mergeMessage);
        Assert.Contains($"Record merged with TRN {deactivatedPerson.Trn}", mergeMessage!.TextContent);

        Assert.NotNull(entry.GetElementByTestId("name"));

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        var taskLink = entry.GetElementByTestId("support-task-link");
        Assert.NotNull(taskLink);
        Assert.Contains(supportTask.SupportTaskReference, taskClosedMessage!.TextContent);

        var reason = entry.GetElementByTestId("change-reason");
        Assert.NotNull(reason);
        Assert.Contains("Some comments", reason!.TextContent);
    }

    [Fact]
    public async Task ProcessRendersCorrectlyForDeactivatedPerson()
    {
        // Arrange
        var (retainedPerson, deactivatedPerson, _, process) = await CreateProcessAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, deactivatedPerson.PersonId);

        // Assert
        var mergeMessage = entry.GetElementByTestId("merge-message");
        Assert.NotNull(mergeMessage);
        Assert.Contains(retainedPerson.Trn!, mergeMessage!.TextContent);
        Assert.Contains("Record merged into TRN", mergeMessage.TextContent);
        Assert.Contains("and deactivated", mergeMessage.TextContent);

        Assert.Null(entry.GetElementByTestId("name"));
    }

    [Fact]
    public async Task ProcessRendersCorrectlyInSupportTaskContext()
    {
        // Arrange
        var (retainedPerson, deactivatedPerson, supportTask, process) = await CreateProcessAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        var mergeMessage = entry.GetElementByTestId("merge-message");
        Assert.NotNull(mergeMessage);
        Assert.Contains(deactivatedPerson.Trn!, mergeMessage!.TextContent);
        Assert.Contains(retainedPerson.Trn!, mergeMessage.TextContent);
        Assert.Contains("merged with TRN", mergeMessage.TextContent);
        Assert.Contains("and deactivated", mergeMessage.TextContent);

        var deactivatedPersonLink = entry.GetElementByTestId("deactivated-person-link");
        Assert.NotNull(deactivatedPersonLink);
        Assert.Contains(deactivatedPerson.Trn!, deactivatedPersonLink!.TextContent);

        var retainedPersonLink = entry.GetElementByTestId("retained-person-link");
        Assert.NotNull(retainedPersonLink);
        Assert.Contains(retainedPerson.Trn!, retainedPersonLink!.TextContent);

        Assert.NotNull(entry.GetElementByTestId("name"));

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        Assert.Null(entry.GetElementByTestId("support-task-link"));
    }

    [Fact]
    public async Task WithoutDetailsChanges_OmitsDetails()
    {
        // Arrange
        var (retainedPerson, _, _, process) = await CreateProcessAsync(withDetailsUpdate: false);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, retainedPerson.PersonId);

        // Assert
        Assert.Null(entry.GetElementByTestId("name"));
    }

    [Fact]
    public async Task WithoutComments_OmitsReasonSection()
    {
        // Arrange
        var (retainedPerson, _, _, process) = await CreateProcessAsync(withComments: false);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, retainedPerson.PersonId);

        // Assert
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task WithSupportTaskCreatedEvent_RendersMessage()
    {
        // Arrange
        var (retainedPerson, _, _, process) = await CreateProcessAsync(withSupportTaskCreatedEvent: true);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, retainedPerson.PersonId);

        // Assert
        var createdMessage = entry.GetElementByTestId("support-task-created-message");
        Assert.NotNull(createdMessage);
        Assert.Contains("TRN request manual checks needed support task", createdMessage!.TextContent);
        Assert.NotNull(entry.GetElementByTestId("created-task-link"));
    }

    private async Task<(Person RetainedPerson, Person DeactivatedPerson, SupportTask SupportTask, Process Process)> CreateProcessAsync(
        bool withDetailsUpdate = true,
        bool withComments = true,
        bool withSupportTaskCreatedEvent = false)
    {
        var retainedPerson = await TestData.CreatePersonAsync();
        var deactivatedPerson = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => { });
        var newLastName = TestData.GenerateChangedLastName(retainedPerson.LastName);

        List<IEvent> events =
        [
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = deactivatedPerson.PersonId,
                MergedWithPersonId = retainedPerson.PersonId,
                Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
                DateOfDeath = null
            }
        ];

        if (withDetailsUpdate)
        {
            events.Add(new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = retainedPerson.PersonId,
                PersonDetails = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, newLastName, retainedPerson.DateOfBirth),
                OldPersonDetails = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, retainedPerson.LastName, retainedPerson.DateOfBirth),
                Changes = PersonDetailsUpdatedEventChanges.LastName
            });
        }

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        var supportTaskEventModel = oldSupportTaskEventModel with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithMerge
        };

        events.Add(new SupportTaskUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTaskReference = supportTask.SupportTaskReference,
            SupportTask = supportTaskEventModel,
            OldSupportTask = oldSupportTaskEventModel,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Outcome,
            Comments = withComments ? "Some comments" : null,
            RejectionReason = null
        });

        if (withSupportTaskCreatedEvent)
        {
            var createdSupportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => { });
            events.Add(new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(createdSupportTask)
            });
        }

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithMerge,
            userId: null,
            changeReason: null,
            events.ToArray());

        return (retainedPerson, deactivatedPerson, supportTask, process);
    }

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName,
        string? middleName,
        string lastName,
        DateOnly? dateOfBirth) => new()
        {
            FirstName = firstName,
            MiddleName = middleName ?? string.Empty,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };
}
