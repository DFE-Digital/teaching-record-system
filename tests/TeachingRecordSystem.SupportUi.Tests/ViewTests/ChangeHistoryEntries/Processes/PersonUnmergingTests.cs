using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonUnmergingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectlyForUnmergedPerson()
    {
        // Arrange
        var (unmergedFromPerson, unmergedPerson, process) = await CreateUnmergeAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, unmergedPerson.PersonId);

        // Assert
        AssertTitle(entry, $"Record un-merged from TRN {unmergedFromPerson.Trn} and reactivated");
    }

    [Fact]
    public async Task ProcessRendersCorrectlyForPersonUnmergedFrom()
    {
        // Arrange
        var (unmergedFromPerson, unmergedPerson, process) = await CreateUnmergeAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, unmergedFromPerson.PersonId);

        // Assert
        AssertTitle(entry, $"TRN {unmergedPerson.Trn} un-merged from this record");
    }

    private async Task<(Person UnmergedFromPerson, Person UnmergedPerson, Process Process)> CreateUnmergeAsync()
    {
        var user = await TestData.CreateUserAsync();
        var unmergedFromPerson = await TestData.CreatePersonAsync();
        var unmergedPerson = await TestData.CreatePersonAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonUnmerging,
            user.UserId,
            changeReason: null,
            new PersonUnmergedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = unmergedPerson.PersonId,
                UnmergedFromPersonId = unmergedFromPerson.PersonId
            });

        return (unmergedFromPerson, unmergedPerson, process);
    }
}
