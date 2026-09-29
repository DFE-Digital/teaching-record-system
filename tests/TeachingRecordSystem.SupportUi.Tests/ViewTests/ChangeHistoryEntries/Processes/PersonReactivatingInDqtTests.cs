using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonReactivatingInDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonReactivatingInDqt,
            SystemUser.Instance.UserId,
            changeReason: null,
            new PersonReactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = PersonReactivatedEventChanges.PersonStatus
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Record reactivated in DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.NotNull(bodyText);
        Assert.Contains($"Record reactivated for {person.FirstName} {person.LastName}", bodyText);

        Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
    }
}
