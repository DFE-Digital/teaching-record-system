using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());
        var user = await TestData.CreateUserAsync();

        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = PersonCreateReason.AnotherReason.GetDisplayName(),
            Details = "Reason detail",
            EvidenceFile = evidenceFile,
            AdditionalInformation = "some additional information"
        };

        // Act
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            user.UserId,
            changeReason,
            CreatePersonDetails(
                firstName: person.FirstName,
                middleName: person.MiddleName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                emailAddress: person.EmailAddress,
                nationalInsuranceNumber: person.NationalInsuranceNumber,
                gender: person.Gender));

        // Assert
        AssertTitle(entry, "Record created");

        entry.AssertSummaryListRowValue("details", "Name", v =>
            Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Date of birth", v =>
            Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Email address", v =>
            Assert.Equal(person.EmailAddress, v.TrimmedText()));
        // The record created view has always shown the National Insurance number in its display form.
        var expectedNino = NationalInsuranceNumber.Parse(person.NationalInsuranceNumber!).ToDisplayString();
        entry.AssertSummaryListRowValue("details", "National Insurance number", v =>
            Assert.Equal(expectedNino, v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Gender", v =>
            Assert.Equal(person.Gender?.GetDisplayName(), v.TrimmedText()));

        entry.AssertSummaryListRowValue("create-reason", "Reason", v =>
            Assert.Equal(changeReason.Reason, v.TrimmedText()));
        entry.AssertSummaryListRowValue("create-reason", "Reason details", v =>
            Assert.Equal(changeReason.Details, v.TrimmedText()));
        entry.AssertSummaryListRowValue("create-reason", "Additional information", v =>
            Assert.Equal(changeReason.AdditionalInformation, v.TrimmedText()));
        entry.AssertSummaryListRowValue("create-reason", "Evidence", v =>
            Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task ProcessWithoutOptionalDetailsOrChangeReason_RendersWithoutThem()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        // Act
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            user.UserId,
            changeReason: null,
            CreatePersonDetails(
                firstName: person.FirstName,
                middleName: person.MiddleName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                emailAddress: null,
                nationalInsuranceNumber: null,
                gender: null));

        // Assert
        AssertTitle(entry, "Record created");

        var details = entry.GetElementByTestId("details");
        Assert.NotNull(details);
        Assert.DoesNotContain("Email address", details.TrimmedText());
        Assert.DoesNotContain("National Insurance number", details.TrimmedText());
        Assert.DoesNotContain("Gender", details.TrimmedText());

        // A record created before change reasons were captured still shows the section, with nothing in it.
        entry.AssertSummaryListRowValue("create-reason", "Reason", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    private async Task<IHtmlElement> PublishPersonCreatedEventAsync(
        Guid personId,
        Guid userId,
        ChangeReasonWithDetailsAndEvidence? changeReason,
        EventModels.PersonDetails details)
    {
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonCreating,
            userId,
            changeReason,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Details = details,
                TrnRequestMetadata = null
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName = "Jane",
        string middleName = "Alice",
        string lastName = "Smith",
        DateOnly? dateOfBirth = null,
        string? emailAddress = "jane.smith@example.com",
        string? nationalInsuranceNumber = "QQ123456C",
        Gender? gender = Gender.Female) =>
        new()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender
        };
}
