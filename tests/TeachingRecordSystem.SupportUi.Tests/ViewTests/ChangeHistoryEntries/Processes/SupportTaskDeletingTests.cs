using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class SupportTaskDeletingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task VisibleFromPersonContext_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var process = await CreateProcessAsync(personId: person.PersonId, oneLoginUserSubject: null);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: person.PersonId);

        // Assert
        AssertTitle(entry, "Support task deleted");
        Assert.Equal($"Task deleted for {person.FirstName} {person.LastName}.", entry.QuerySelector(".govuk-body")?.TrimmedText());
    }

    [Fact]
    public async Task VisibleFromOneLoginContext_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var process = await CreateProcessAsync(personId: person.PersonId, oneLoginUserSubject: oneLoginUser.Subject);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "oneLogin", oneLoginSubject: oneLoginUser.Subject);

        // Assert
        AssertTitle(entry, "Support task deleted");
        Assert.Equal($"Task deleted for {person.FirstName} {person.LastName}.", entry.QuerySelector(".govuk-body")?.TrimmedText());
    }

    [Fact]
    public async Task WithoutLinkedPerson_FallsBackToOneLoginEmail()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var process = await CreateProcessAsync(personId: null, oneLoginUserSubject: oneLoginUser.Subject);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "oneLogin", oneLoginSubject: oneLoginUser.Subject);

        // Assert
        AssertTitle(entry, "Support task deleted");
        Assert.Equal($"Task deleted for {oneLoginUser.EmailAddress}.", entry.QuerySelector(".govuk-body")?.TrimmedText());
    }

    private Task<Core.DataStore.Postgres.Models.Process> CreateProcessAsync(Guid? personId, string? oneLoginUserSubject)
    {
        var supportTask = new EventModels.SupportTask
        {
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            Status = SupportTaskStatus.Open,
            OneLoginUserSubject = oneLoginUserSubject,
            PersonId = personId,
            Data = new NpqTrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            ZendeskTickets = [],
            Outcome = null
        };

        return TestData.CreateProcessAsync(
            ProcessType.SupportTaskDeleting,
            changeReason: null,
            events: new SupportTaskDeletedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                SupportTask = supportTask,
                ReasonDetail = null
            });
    }
}
