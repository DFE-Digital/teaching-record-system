using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonImportingIntoDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    private static readonly DateOnly _dateOfBirth = new(1990, 1, 2);
    private static readonly DateOnly _dateOfDeath = new(2024, 5, 6);
    private static readonly DateOnly _qtsDate = new(2020, 3, 4);
    private static readonly DateOnly _eytsDate = new(2021, 4, 5);

    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithDateOfBirth(_dateOfBirth)
            .WithNationalInsuranceNumber()
            .WithEmailAddress()
            .WithGender()
            .WithQts(_qtsDate)
            .WithEyts(_eytsDate)
            .WithInductionStatus(InductionStatus.Passed));
        var user = await TestData.CreateUserAsync();

        // Act
        var entry = await PublishPersonImportedIntoDqtEventAsync(
            person.PersonId,
            user.UserId,
            CreateDqtPersonDetails(
                trn: person.Trn,
                firstName: person.FirstName,
                middleName: person.MiddleName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                emailAddress: person.EmailAddress,
                nationalInsuranceNumber: person.NationalInsuranceNumber,
                gender: person.Gender,
                dateOfDeath: _dateOfDeath,
                qtsDate: person.QtsDate,
                eytsDate: person.EytsDate,
                inductionStatus: person.InductionStatus,
                dqtInductionStatus: person.InductionStatus.ToDqtInductionStatus(out _)));

        // Assert
        AssertTitle(entry, "Record migrated to DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Record migrated for", bodyText);
        Assert.Contains($"{person.FirstName} {person.MiddleName} {person.LastName}", bodyText);

        var recordDetails = entry.QuerySelector("details");
        Assert.NotNull(recordDetails);
        var recordDetailsSummary = recordDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Record details", recordDetailsSummary?.TrimmedText());

        entry.AssertSummaryListHasRows(
            ("TRN", person.Trn!),
            ("Name", $"{person.FirstName} {person.MiddleName} {person.LastName}"),
            ("Date of birth", person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat)),
            ("Email address", person.EmailAddress),
            ("National Insurance number", person.NationalInsuranceNumber),
            ("Gender", person.Gender?.GetDisplayName()),
            ("Date of death", _dateOfDeath.ToString(WebConstants.DateDisplayFormat)),
            ("QTS held since", person.QtsDate!.Value.ToString(WebConstants.DateDisplayFormat)),
            ("EYTS held since", person.EytsDate!.Value.ToString(WebConstants.DateDisplayFormat)),
            ("Induction status", person.InductionStatus.GetDisplayName()),
            ("DQT induction status", person.InductionStatus.ToDqtInductionStatus(out _)));
    }

    [Fact]
    public async Task WithoutOptionalDetails_OmitsRows()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var entry = await PublishPersonImportedIntoDqtEventAsync(
            person.PersonId,
            SystemUser.Instance.UserId,
            CreateDqtPersonDetails(
                trn: person.Trn,
                firstName: string.Empty,
                middleName: string.Empty,
                lastName: string.Empty,
                dateOfBirth: null,
                emailAddress: null,
                nationalInsuranceNumber: null,
                gender: null,
                dateOfDeath: null,
                qtsDate: null,
                eytsDate: null,
                inductionStatus: null,
                dqtInductionStatus: null));

        // Assert
        AssertTitle(entry, "Record migrated to DQT");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Record migrated for", bodyText);

        var recordDetails = entry.QuerySelector("details");
        Assert.NotNull(recordDetails);

        entry.AssertSummaryListHasRows(
            ("TRN", person.Trn!));
    }

    private async Task<IHtmlElement> PublishPersonImportedIntoDqtEventAsync(
        Guid personId,
        Guid userId,
        EventModels.DqtPersonDetails details)
    {
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonImportingIntoDqt,
            userId,
            changeReason: null,
            new PersonImportedIntoDqtEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Details = details
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private static EventModels.DqtPersonDetails CreateDqtPersonDetails(
        string? trn = "1234567",
        string firstName = "Jane",
        string middleName = "Alice",
        string lastName = "Smith",
        DateOnly? dateOfBirth = null,
        string? emailAddress = "jane.smith@example.com",
        string? nationalInsuranceNumber = "QQ123456C",
        Gender? gender = Gender.Female,
        DateOnly? dateOfDeath = null,
        DateOnly? qtsDate = null,
        DateOnly? eytsDate = null,
        DateOnly? qtlsDate = null,
        QtlsStatus qtlsStatus = QtlsStatus.None,
        InductionStatus? inductionStatus = null,
        string? dqtInductionStatus = null) =>
        new()
        {
            Trn = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender,
            DateOfDeath = dateOfDeath,
            QtsDate = qtsDate,
            EytsDate = eytsDate,
            QtlsDate = qtlsDate,
            QtlsStatus = qtlsStatus,
            InductionStatus = inductionStatus,
            DqtInductionStatus = dqtInductionStatus
        };
}
