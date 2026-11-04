using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonMigratingFromDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(new DateOnly(1990, 1, 2))
            .WithNationalInsuranceNumber()
            .WithEmailAddress()
            .WithGender());

        // Act
        var entry = await PublishPersonMigratedEventAsync(
            person.PersonId,
            person.Trn,
            new EventModels.PersonDetails
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                EmailAddress = person.EmailAddress,
                NationalInsuranceNumber = person.NationalInsuranceNumber,
                Gender = person.Gender
            });

        // Assert
        AssertTitle(entry, "Record migrated from DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Record migrated for", bodyText);
        Assert.Contains($"{person.FirstName} {person.MiddleName} {person.LastName}", bodyText);

        var recordDetails = entry.QuerySelector("details");
        Assert.NotNull(recordDetails);
        Assert.Equal("Record details", recordDetails.GetElementsByTagName("summary").SingleOrDefault()?.TrimmedText());

        entry.AssertSummaryListHasRows(
            ("TRN", person.Trn!),
            ("Name", $"{person.FirstName} {person.MiddleName} {person.LastName}"),
            ("Date of birth", person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat)),
            ("Email address", person.EmailAddress),
            ("National Insurance number", person.NationalInsuranceNumber),
            ("Gender", person.Gender?.GetDisplayName()));
    }

    [Fact]
    public async Task WithoutOptionalDetails_OmitsRows()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var entry = await PublishPersonMigratedEventAsync(
            person.PersonId,
            person.Trn,
            new EventModels.PersonDetails
            {
                FirstName = string.Empty,
                MiddleName = string.Empty,
                LastName = string.Empty,
                DateOfBirth = null,
                EmailAddress = null,
                NationalInsuranceNumber = null,
                Gender = null
            });

        // Assert
        AssertTitle(entry, "Record migrated from DQT");
        Assert.NotNull(entry.QuerySelector("details"));
        entry.AssertSummaryListHasRows(("TRN", person.Trn!));
    }

    private async Task<IHtmlElement> PublishPersonMigratedEventAsync(
        Guid personId,
        string? trn,
        EventModels.PersonDetails attributes)
    {
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonMigratingFromDqt,
            SystemUser.Instance.UserId,
            changeReason: null,
            new PersonMigratedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Trn = trn,
                PersonAttributes = attributes
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
