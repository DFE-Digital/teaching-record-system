using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class SupportTaskAllocatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task AssignedToUser_FromPersonContext_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(person.PersonId, oneLoginUserSubject: null, assignedToUser.UserId);

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
        var process = await CreateProcessAsync(person.PersonId, oneLoginUserSubject: null, assignedToUser.UserId, supportTaskReference: "TEST-ST-1");

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
        var process = await CreateProcessAsync(personId: null, oneLoginUser.Subject, assignedToUser.UserId);

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
    public async Task AssignedToUser_FromSupportTaskContext_LinksToOneLogin()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(personId: null, oneLoginUser.Subject, assignedToUser.UserId, supportTaskReference: "TEST-ST-1");

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        var oneLoginLink = message!.QuerySelector("[data-testid='one-login-link']") as IHtmlAnchorElement;
        Assert.NotNull(oneLoginLink);
        Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
    }

    [Fact]
    public async Task Unassigned_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var process = await CreateProcessAsync(person.PersonId, oneLoginUserSubject: null, assignedToUserId: null);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: person.PersonId);

        // Assert
        AssertTitle(entry, "Support task assigned");
        var message = entry.GetElementByTestId("task-assigned-message");
        Assert.NotNull(message);
        Assert.Contains("unassigned", message!.TextContent);
    }

    [Fact]
    public async Task AssignedToUser_WithBothPersonAndOneLogin_LinksToBoth()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var assignedToUser = await TestData.CreateUserAsync();
        var process = await CreateProcessAsync(person.PersonId, oneLoginUser.Subject, assignedToUser.UserId, supportTaskReference: "TEST-ST-1");

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
        Guid? personId,
        string? oneLoginUserSubject,
        Guid? assignedToUserId,
        string supportTaskReference = "TEST-ST-1")
    {
        var oldSupportTask = new EventModels.SupportTask
        {
            SupportTaskReference = supportTaskReference,
            SupportTaskType = SupportTaskType.TrnRequest,
            Status = SupportTaskStatus.Open,
            OneLoginUserSubject = oneLoginUserSubject,
            PersonId = personId,
            Data = new TrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            ZendeskTickets = [],
            Outcome = null
        };

        var supportTask = oldSupportTask with { AssignedToUserId = assignedToUserId };

        return TestData.CreateProcessAsync(
            ProcessType.SupportTaskAllocating,
            changeReason: null,
            events: new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.AssignedToUserId,
                OldSupportTask = oldSupportTask,
                SupportTask = supportTask,
                Comments = null,
                RejectionReason = null
            });
    }
}
