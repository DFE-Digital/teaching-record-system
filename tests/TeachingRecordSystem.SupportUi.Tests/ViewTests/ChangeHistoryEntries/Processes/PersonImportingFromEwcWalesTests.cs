using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonImportingFromEwcWalesTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ImportThatCreatedAPerson_RendersTheRecordDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonImportingFromEwcWales,
            SystemUser.Instance.UserId,
            changeReason: null,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = null
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Record imported from EWC Wales");

        Assert.Equal(
            "Record created from the EWC Wales import.",
            entry.GetElementByTestId("import-summary")?.TrimmedText());

        entry.AssertSummaryListRowValue("details", "Name", v =>
            Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Date of birth", v =>
            Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
    }
}


