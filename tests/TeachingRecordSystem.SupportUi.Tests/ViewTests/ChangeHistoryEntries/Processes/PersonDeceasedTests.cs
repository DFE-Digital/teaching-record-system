using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonDeceasedTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = PersonDeactivateReason.AnotherReason.GetDisplayName(),
            Details = "Reason detail",
            EvidenceFile = evidenceFile,
            AdditionalInformation = "some additional information"
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonDeceased,
            user.UserId,
            changeReason,
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = PersonDeactivatedEventChanges.PersonStatus,
                MergedWithPersonId = null,
                DateOfDeath = TimeProvider.Today
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Record deactivated");

        var name = entry.GetElementByTestId("name");
        Assert.NotNull(name);
        Assert.Equal($"{person.FirstName} {person.LastName} marked as deceased", name!.TrimmedText());

        entry.AssertSummaryListRowValue("change-reason", "Reason", v =>
            Assert.Equal(changeReason.Details, v.TrimmedText()));
        entry.AssertSummaryListRowValue("change-reason", "Additional information", v =>
            Assert.Equal(changeReason.AdditionalInformation, v.TrimmedText()));
        entry.AssertSummaryListRowValue("change-reason", "Evidence", v =>
            Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task ProcessWithoutChangeReason_RendersWithoutReasonSection()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonDeceased,
            user.UserId,
            changeReason: null,
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = PersonDeactivatedEventChanges.PersonStatus,
                MergedWithPersonId = null,
                DateOfDeath = TimeProvider.Today
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, person.PersonId);

        // Assert
        AssertTitle(entry, "Record deactivated");

        Assert.Null(entry.GetElementByTestId("change-reason"));
    }
}
