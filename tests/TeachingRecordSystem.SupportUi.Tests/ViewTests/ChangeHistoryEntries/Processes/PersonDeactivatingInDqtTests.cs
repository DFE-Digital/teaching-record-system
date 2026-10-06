using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonDeactivatingInDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonDeactivatingInDqt,
            SystemUser.Instance.UserId,
            changeReason: null,
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = PersonDeactivatedEventChanges.PersonStatus,
                MergedWithPersonId = null,
                DateOfDeath = null
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Record deactivated in DQT");

        Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
    }
}
