using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
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
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

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
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
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
        var entry = await GetEntryHtmlAsync(process.ProcessId, mergedWithPerson.PersonId);

        // Assert
        AssertTitle(entry, $"Record merged with TRN {person.Trn}");
        Assert.Empty(entry.QuerySelectorAll(".govuk-summary-list"));
    }
}
