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
        AssertTitle(entry, "TRN request support task deleted by Admin");
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
        AssertTitle(entry, "TRN request support task deleted by Admin");
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
        AssertTitle(entry, "TRN request support task deleted by Admin");
    }

    private Task<Core.DataStore.Postgres.Models.Process> CreateProcessAsync(Guid? personId, string? oneLoginUserSubject)
    {
        var supportTask = new EventModels.SupportTask
        {
            SupportTaskReference = "TEST-ST-1",
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
