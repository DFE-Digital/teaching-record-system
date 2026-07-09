using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonCreatingInDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());
        var user = await TestData.CreateUserAsync();

        // Act
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            user.UserId,
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

        entry.AssertSummaryListHasRows(
            ("Name", $"{person.FirstName} {person.MiddleName} {person.LastName}"),
            ("Date of birth", person.DateOfBirth.ToString(WebConstants.DateDisplayFormat)),
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
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            SystemUser.Instance.UserId,
            CreatePersonDetails(
                firstName: string.Empty,
                middleName: string.Empty,
                lastName: string.Empty,
                dateOfBirth: null,
                emailAddress: null,
                nationalInsuranceNumber: null,
                gender: null));

        // Assert
        AssertTitle(entry, "Record created");
        entry.AssertSummaryListHasRows();
    }

    private async Task<IHtmlElement> PublishPersonCreatedEventAsync(
        Guid personId,
        Guid userId,
        EventModels.PersonDetails details)
    {
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonCreatingInDqt,
            userId,
            changeReason: null,
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
