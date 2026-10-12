using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class TeacherPensionsDuplicateSupportTaskResolvingWithoutMergeTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectlyForPerson()
    {
        // Arrange
        var (person, supportTask, process) = await CreateProcessAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Teachers’ Pensions potential duplicate task resolved");

        var keptSeparateMessage = entry.GetElementByTestId("kept-separate-message");
        Assert.NotNull(keptSeparateMessage);
        Assert.Contains("kept as a separate record", keptSeparateMessage!.TextContent);
        Assert.Null(entry.GetElementByTestId("person-link"));

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        Assert.NotNull(entry.GetElementByTestId("support-task-link"));

        var reason = entry.GetElementByTestId("change-reason");
        Assert.NotNull(reason);
        Assert.Contains("Some comments", reason!.TextContent);
    }

    [Fact]
    public async Task ProcessRendersCorrectlyInSupportTaskContext()
    {
        // Arrange
        var (person, supportTask, process) = await CreateProcessAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        var keptSeparateMessage = entry.GetElementByTestId("kept-separate-message");
        Assert.NotNull(keptSeparateMessage);
        var personLink = entry.GetElementByTestId("person-link");
        Assert.NotNull(personLink);
        Assert.Contains(person.Trn!, personLink!.TextContent);

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        Assert.Null(entry.GetElementByTestId("support-task-link"));
    }

    [Fact]
    public async Task WithoutComments_OmitsReasonSection()
    {
        // Arrange
        var (person, _, process) = await CreateProcessAsync(withComments: false);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task WithSupportTaskCreatedEvent_RendersMessage()
    {
        // Arrange
        var (person, _, process) = await CreateProcessAsync(withSupportTaskCreatedEvent: true);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        var createdMessage = entry.GetElementByTestId("support-task-created-message");
        Assert.NotNull(createdMessage);
        Assert.Contains("TRN request manual checks needed support task", createdMessage!.TextContent);
        Assert.NotNull(entry.GetElementByTestId("created-task-link"));
    }

    [Fact]
    public async Task WithOneLoginUserUpdatedEvent_InPersonContext_RendersOneLoginLinkOnly()
    {
        // Arrange
        var (person, _, process, oneLoginUser) = await CreateProcessWithOneLoginMatchAsync(person: null);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        var message = entry.GetElementByTestId("one-login-matched-message");
        Assert.NotNull(message);
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        Assert.NotNull(oneLoginLink);
        Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
        Assert.Null(entry.GetElementByTestId("matched-person-link"));
        Assert.Contains("matched to record", message!.TextContent);
    }

    [Fact]
    public async Task WithOneLoginUserUpdatedEvent_InOneLoginContext_RendersPersonLinkOnly()
    {
        // Arrange
        var (person, _, process, oneLoginUser) = await CreateProcessWithOneLoginMatchAsync(person: null);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "oneLogin", oneLoginSubject: oneLoginUser.Subject);

        // Assert
        var message = entry.GetElementByTestId("one-login-matched-message");
        Assert.NotNull(message);
        Assert.Null(entry.GetElementByTestId("one-login-link"));
        var matchedPersonLink = entry.GetElementByTestId("matched-person-link");
        Assert.NotNull(matchedPersonLink);
        Assert.Contains(person.Trn!, matchedPersonLink!.TextContent);
    }

    [Fact]
    public async Task WithOneLoginUserUpdatedEvent_InSupportTaskContext_RendersBothLinks()
    {
        // Arrange
        var (person, supportTask, process, oneLoginUser) = await CreateProcessWithOneLoginMatchAsync(person: null);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        var message = entry.GetElementByTestId("one-login-matched-message");
        Assert.NotNull(message);
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        Assert.NotNull(oneLoginLink);
        var matchedPersonLink = entry.GetElementByTestId("matched-person-link");
        Assert.NotNull(matchedPersonLink);
        Assert.Contains(person.Trn!, matchedPersonLink!.TextContent);
    }

    [Fact]
    public async Task WithoutOneLoginUserUpdatedEvent_OmitsMessage()
    {
        // Arrange
        var (person, _, process) = await CreateProcessAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        Assert.Null(entry.GetElementByTestId("one-login-matched-message"));
    }

    private async Task<(Person Person, SupportTask SupportTask, Process Process)> CreateProcessAsync(
        bool withComments = true,
        bool withSupportTaskCreatedEvent = false)
    {
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => { });
        var person = await WithDbContextAsync(async dbContext =>
            (await dbContext.Persons.FindAsync(supportTask.PersonId))!);

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        var supportTaskEventModel = oldSupportTaskEventModel with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithoutMerge
        };

        List<IEvent> events =
        [
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                SupportTask = supportTaskEventModel,
                OldSupportTask = oldSupportTaskEventModel,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Outcome,
                Comments = withComments ? "Some comments" : null,
                RejectionReason = null
            }
        ];

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
            ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge,
            userId: null,
            changeReason: null,
            events.ToArray());

        return (person, supportTask, process);
    }

    private async Task<(Person Person, SupportTask SupportTask, Process Process, OneLoginUser OneLoginUser)> CreateProcessWithOneLoginMatchAsync(Person? person)
    {
        var (createdPerson, supportTask, _) = await CreateProcessAsync();
        var actualPerson = person ?? createdPerson;

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(actualPerson);

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        var supportTaskEventModel = oldSupportTaskEventModel with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithoutMerge
        };

        List<IEvent> events =
        [
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                SupportTask = supportTaskEventModel,
                OldSupportTask = oldSupportTaskEventModel,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Outcome,
                Comments = "Some comments",
                RejectionReason = null
            },
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            }
        ];

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge,
            userId: null,
            changeReason: null,
            events.ToArray());

        return (actualPerson, supportTask, process, oneLoginUser);
    }
}
