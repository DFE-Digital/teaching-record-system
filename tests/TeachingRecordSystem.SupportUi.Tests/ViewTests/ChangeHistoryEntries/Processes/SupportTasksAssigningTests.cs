using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class SupportTasksAssigningTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task AssignedToUser_FromPersonContext_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(
            (person.PersonId, oneLoginUserSubject: null, assignedToUser.UserId, "TEST-ST-1"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: person.PersonId);

        // Assert
        AssertTitle(entry, "Support task assigned");
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Contains(assignedToUser.Name, message!.TextContent);
        Assert.Contains($"{person.FirstName} {person.LastName}", message.TextContent);
        Assert.Null(message.QuerySelector("[data-testid='person-link']"));
    }

    [Fact]
    public async Task AssignedToUser_FromSupportTaskContext_LinksToPerson()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(
            (person.PersonId, oneLoginUserSubject: null, assignedToUser.UserId, "TEST-ST-1"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "Support task assigned");
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Null(message!.QuerySelector("[data-testid='support-task-link']"));
        Assert.Contains(assignedToUser.Name, message.TextContent);

        var personLink = message.QuerySelector("[data-testid='person-link']") as IHtmlAnchorElement;
        Assert.NotNull(personLink);
        Assert.Contains($"{person.FirstName} {person.LastName}", personLink!.TextContent);
    }

    [Fact]
    public async Task AssignedToUser_FromOneLoginContext_RendersCorrectly()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(
            (personId: null, oneLoginUser.Subject, assignedToUser.UserId, "TEST-ST-1"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "oneLogin", oneLoginSubject: oneLoginUser.Subject);

        // Assert
        AssertTitle(entry, "Support task assigned");
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Contains(assignedToUser.Name, message!.TextContent);
        Assert.Contains(oneLoginUser.EmailAddress!, message.TextContent);
        Assert.Null(message.QuerySelector("[data-testid='one-login-link']"));
    }

    [Fact]
    public async Task Unassigned_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var process = await CreateProcessAsync(
            (person.PersonId, oneLoginUserSubject: null, assignedToUserId: null, "TEST-ST-1"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: person.PersonId);

        // Assert
        AssertTitle(entry, "Support task assigned");
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Contains("unassigned", message!.TextContent);
    }

    [Fact]
    public async Task MultipleTasksInProcess_PersonContext_OnlyShowsRelevantTaskInfo()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var person2 = await TestData.CreatePersonAsync();
        var assignedToUser1 = await TestData.CreateUserAsync();
        var assignedToUser2 = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(
            (person1.PersonId, oneLoginUserSubject: null, assignedToUser1.UserId, "TEST-ST-1"),
            (person2.PersonId, oneLoginUserSubject: null, assignedToUser2.UserId, "TEST-ST-2"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: person1.PersonId);

        // Assert
        AssertTitle(entry, "Support task assigned");
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Contains(assignedToUser1.Name, message!.TextContent);
        Assert.Contains($"{person1.FirstName} {person1.LastName}", message.TextContent);
        Assert.DoesNotContain(assignedToUser2.Name, message.TextContent);
        Assert.DoesNotContain($"{person2.FirstName} {person2.LastName}", message.TextContent);
    }

    [Fact]
    public async Task MultipleTasksInProcess_SupportTaskContext_OnlyShowsRelevantTaskInfo()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var person2 = await TestData.CreatePersonAsync();
        var assignedToUser1 = await TestData.CreateUserAsync();
        var assignedToUser2 = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(
            (person1.PersonId, oneLoginUserSubject: null, assignedToUser1.UserId, "TEST-ST-1"),
            (person2.PersonId, oneLoginUserSubject: null, assignedToUser2.UserId, "TEST-ST-2"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-2");

        // Assert
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Contains(assignedToUser2.Name, message!.TextContent);
        Assert.Contains($"{person2.FirstName} {person2.LastName}", message.TextContent);
        Assert.DoesNotContain(assignedToUser1.Name, message.TextContent);
        Assert.DoesNotContain($"{person1.FirstName} {person1.LastName}", message.TextContent);
    }

    [Fact]
    public async Task AssignedToUser_WithBothPersonAndOneLogin_LinksToBoth()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(
            (person.PersonId, oneLoginUser.Subject, assignedToUser.UserId, "TEST-ST-1"));

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);

        var personLink = message!.QuerySelector("[data-testid='person-link']") as IHtmlAnchorElement;
        Assert.NotNull(personLink);
        Assert.Contains($"{person.FirstName} {person.LastName}", personLink!.TextContent);

        var oneLoginLink = message.QuerySelector("[data-testid='one-login-link']") as IHtmlAnchorElement;
        Assert.NotNull(oneLoginLink);
        Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
    }

    private Task<Core.DataStore.Postgres.Models.Process> CreateProcessAsync(
        params (Guid? PersonId, string? OneLoginUserSubject, Guid? AssignedToUserId, string SupportTaskReference)[] tasks)
    {
        var events = tasks.Select(task =>
        {
            var oldSupportTask = new EventModels.SupportTask
            {
                SupportTaskReference = task.SupportTaskReference,
                SupportTaskType = SupportTaskType.TrnRequest,
                Status = SupportTaskStatus.Open,
                OneLoginUserSubject = task.OneLoginUserSubject,
                PersonId = task.PersonId,
                Data = new TrnRequestData(),
                SourceApplicationUserId = null,
                ResolveJourneySavedState = null,
                AssignedToUserId = null,
                ZendeskTickets = [],
                Outcome = null
            };

            var supportTask = oldSupportTask with { AssignedToUserId = task.AssignedToUserId };

            return (IEvent)new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = task.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.AssignedToUserId,
                OldSupportTask = oldSupportTask,
                SupportTask = supportTask,
                Comments = null,
                RejectionReason = null
            };
        }).ToArray();

        return TestData.CreateProcessAsync(
            ProcessType.SupportTasksAssigning,
            changeReason: null,
            events: events);
    }
}
