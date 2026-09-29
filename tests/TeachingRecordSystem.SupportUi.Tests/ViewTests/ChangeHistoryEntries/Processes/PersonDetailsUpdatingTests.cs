using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonDetailsUpdatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName)]
    [InlineData(PersonDetailsUpdatedEventChanges.MiddleName)]
    [InlineData(PersonDetailsUpdatedEventChanges.LastName)]
    [InlineData(PersonDetailsUpdatedEventChanges.DateOfBirth)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber)]
    [InlineData(PersonDetailsUpdatedEventChanges.Gender)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName
        | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress
        | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber | PersonDetailsUpdatedEventChanges.Gender)]
    public async Task ProcessRendersExpectedContent(PersonDetailsUpdatedEventChanges changes)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var oldDetails = new EventModels.PersonDetails
        {
            FirstName = "Alfred",
            MiddleName = "The",
            LastName = "Great",
            DateOfBirth = TimeProvider.Today.AddYears(-30),
            EmailAddress = "old@email-address.com",
            NationalInsuranceNumber = "AB123456D",
            Gender = Gender.Male
        };

        var newDetails = new EventModels.PersonDetails
        {
            FirstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : oldDetails.FirstName,
            MiddleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : oldDetails.MiddleName,
            LastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : oldDetails.LastName,
            DateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? TimeProvider.Today.AddYears(-20) : oldDetails.DateOfBirth,
            EmailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : oldDetails.EmailAddress,
            NationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : oldDetails.NationalInsuranceNumber,
            Gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : oldDetails.Gender
        };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = PersonDetailsChangeReason.AnotherReason.GetDisplayName(),
            Details = "Reason detail",
            EvidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" },
            AdditionalInformation = "some additional information"
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonDetailsUpdating,
            user.UserId,
            changeReason,
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                PersonDetails = newDetails,
                OldPersonDetails = oldDetails,
                Changes = changes
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Record updated");

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            Assert.Equal(
                $"Name changed from {oldDetails.FirstName} {oldDetails.MiddleName} {oldDetails.LastName} to {newDetails.FirstName} {newDetails.MiddleName} {newDetails.LastName}",
                entry.GetElementByTestId("name")?.TrimmedText());
        }
        else
        {
            Assert.Null(entry.GetElementByTestId("name"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth))
        {
            Assert.Equal(
                $"Date of birth changed from {oldDetails.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat)} to {newDetails.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat)}",
                entry.GetElementByTestId("date-of-birth")?.TrimmedText());
        }
        else
        {
            Assert.Null(entry.GetElementByTestId("date-of-birth"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress))
        {
            Assert.Equal(
                $"Email address changed from {oldDetails.EmailAddress} to {newDetails.EmailAddress}",
                entry.GetElementByTestId("email-address")?.TrimmedText());
        }
        else
        {
            Assert.Null(entry.GetElementByTestId("email-address"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber))
        {
            var oldNino = NationalInsuranceNumber.Parse(oldDetails.NationalInsuranceNumber!).ToDisplayString();
            var newNino = NationalInsuranceNumber.Parse(newDetails.NationalInsuranceNumber!).ToDisplayString();
            Assert.Equal(
                $"National Insurance number changed from {oldNino} to {newNino}",
                entry.GetElementByTestId("national-insurance-number")?.TrimmedText());
        }
        else
        {
            Assert.Null(entry.GetElementByTestId("national-insurance-number"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender))
        {
            Assert.Equal(
                $"Gender changed from {oldDetails.Gender?.GetDisplayName()} to {newDetails.Gender?.GetDisplayName()}",
                entry.GetElementByTestId("gender")?.TrimmedText());
        }
        else
        {
            Assert.Null(entry.GetElementByTestId("gender"));
        }

        entry.AssertSummaryListRowValue("change-reason", "Reason", v =>
            Assert.Equal(changeReason.Details, v.TrimmedText()));
        entry.AssertSummaryListRowValue("change-reason", "Additional information", v =>
            Assert.Equal(changeReason.AdditionalInformation, v.TrimmedText()));
        entry.AssertSummaryListRowValue("change-reason", "Evidence", v =>
            Assert.Equal($"{changeReason.EvidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task ProcessWithNoChangeReason_DoesNotRenderChangeReasonSection()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var oldDetails = new EventModels.PersonDetails
        {
            FirstName = "Alfred",
            MiddleName = "",
            LastName = "Great",
            DateOfBirth = TimeProvider.Today.AddYears(-30),
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };

        var newDetails = oldDetails with { FirstName = "Megan" };

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonDetailsUpdating,
            user.UserId,
            changeReason: null,
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                PersonDetails = newDetails,
                OldPersonDetails = oldDetails,
                Changes = PersonDetailsUpdatedEventChanges.FirstName
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Record updated");

        Assert.NotNull(entry.GetElementByTestId("name"));
        Assert.Null(entry.GetElementByTestId("date-of-birth"));
        Assert.Null(entry.GetElementByTestId("email-address"));
        Assert.Null(entry.GetElementByTestId("national-insurance-number"));
        Assert.Null(entry.GetElementByTestId("gender"));
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }
}
