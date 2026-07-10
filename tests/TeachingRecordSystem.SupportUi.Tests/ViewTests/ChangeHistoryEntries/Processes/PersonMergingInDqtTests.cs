using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonMergingInDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly_ForDeactivatedRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var mergedWithPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            // Note Person.MergedWithPersonId isn't set for merges that happened in DQT
            await dbContext.SaveChangesAsync();
        });

        var @event = new PersonDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
            MergedWithPersonId = mergedWithPerson.PersonId,
            DateOfDeath = null,
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event);


        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            new Dictionary<string, object?>
            {
                [nameof(ChangeHistoryEntryViewModel.PersonId)] = person.PersonId,
                [nameof(ChangeHistoryEntryViewModel.PersonInfo)] = new Dictionary<Guid, TeachingRecordSystem.Core.PersonInfo>()
                {
                    [person.PersonId] = new(person.PersonId, person.Trn),
                    [mergedWithPerson.PersonId] = new(mergedWithPerson.PersonId, mergedWithPerson.Trn)
                }
            });

        // Assert
        AssertTitle(entry, $"Record merged with TRN {mergedWithPerson.Trn} and deactivated");
        Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
    }

    [Fact]
    public async Task ProcessRendersCorrectly_ForRetainedRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var mergedWithPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            // Note Person.MergedWithPersonId isn't set for merges that happened in DQT
            await dbContext.SaveChangesAsync();
        });

        var @event = new PersonDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
            MergedWithPersonId = mergedWithPerson.PersonId,
            DateOfDeath = null,
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonMergingInDqt, user.UserId, changeReason: null, @event);

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            new Dictionary<string, object?>
            {
                [nameof(ChangeHistoryEntryViewModel.PersonId)] = mergedWithPerson.PersonId,
                [nameof(ChangeHistoryEntryViewModel.PersonInfo)] = new Dictionary<Guid, TeachingRecordSystem.Core.PersonInfo>()
                {
                    [person.PersonId] = new(person.PersonId, person.Trn),
                    [mergedWithPerson.PersonId] = new(mergedWithPerson.PersonId, mergedWithPerson.Trn)
                }
            });

        // Assert
        AssertTitle(entry, $"Record merged with TRN {person.Trn}");
        Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
    }
}
