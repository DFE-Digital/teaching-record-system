using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonUpdatingInDqtTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    private static readonly DateOnly _dateOfBirth = new(1990, 1, 2);
    private static readonly DateOnly _dateOfDeath = new(2024, 5, 6);
    private static readonly DateOnly _qtsDate = new(2020, 3, 4);
    private static readonly DateOnly _eytsDate = new(2021, 4, 5);
    private static readonly DateOnly _qtlsDate = new(2022, 5, 6);

    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var oldInductionStatus = InductionStatus.Exempt;

        var details = CreatePersonDetails(
            firstName: "Jane",
            middleName: "Alice",
            lastName: "Smith",
            dateOfBirth: _dateOfBirth,
            emailAddress: "jane.smith@example.com",
            nationalInsuranceNumber: "QQ123456C",
            gender: Gender.Female,
            trn: "1234567",
            dateOfDeath: _dateOfDeath,
            qtsDate: _qtsDate,
            eytsDate: _eytsDate,
            qtlsDate: _qtlsDate,
            qtlsStatus: QtlsStatus.Active,
            inductionStatus: inductionStatus,
            dqtInductionStatus: inductionStatus.ToDqtInductionStatus(out _));

        var oldDetails = CreatePersonDetails(
            firstName: "Janet",
            middleName: "Anne",
            lastName: "Jones",
            dateOfBirth: new DateOnly(1989, 2, 3),
            emailAddress: "janet.jones@example.com",
            nationalInsuranceNumber: "QQ765432D",
            gender: Gender.Male,
            trn: "7654321",
            dateOfDeath: new DateOnly(2023, 4, 5),
            qtsDate: new DateOnly(2018, 1, 2),
            eytsDate: new DateOnly(2019, 2, 3),
            qtlsDate: new DateOnly(2020, 3, 4),
            qtlsStatus: QtlsStatus.Expired,
            inductionStatus: oldInductionStatus,
            dqtInductionStatus: oldInductionStatus.ToDqtInductionStatus(out _));

        // Act
        var entry = await PublishPersonUpdatedInDqtEventAsync(PersonUpdatedInDqtEventChanges.All, details, oldDetails);

        // Assert
        AssertTitle(entry, "Record updated");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("TRN", details.Trn),
            ("Name", $"{details.FirstName} {details.MiddleName} {details.LastName}"),
            ("Date of birth", details.DateOfBirth?.ToString(WebConstants.DateDisplayFormat)),
            ("Email address", details.EmailAddress),
            ("National Insurance number", details.NationalInsuranceNumber),
            ("Gender", details.Gender?.GetDisplayName()),
            ("Date of death", details.DateOfDeath?.ToString(WebConstants.DateDisplayFormat)),
            ("QTS held since", details.QtsDate?.ToString(WebConstants.DateDisplayFormat)),
            ("EYTS held since", details.EytsDate?.ToString(WebConstants.DateDisplayFormat)),
            ("QTLS held since", details.QtlsDate?.ToString(WebConstants.DateDisplayFormat)),
            ("Qualified teacher learning and skills status (QTLS)", details.QtlsStatus.GetDisplayName()),
            ("Induction status", details.InductionStatus?.GetDisplayName()),
            ("DQT induction status", details.DqtInductionStatus));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("TRN", oldDetails.Trn),
            ("Name", $"{oldDetails.FirstName} {oldDetails.MiddleName} {oldDetails.LastName}"),
            ("Date of birth", oldDetails.DateOfBirth?.ToString(WebConstants.DateDisplayFormat)),
            ("Email address", oldDetails.EmailAddress),
            ("National Insurance number", oldDetails.NationalInsuranceNumber),
            ("Gender", oldDetails.Gender?.GetDisplayName()),
            ("Date of death", oldDetails.DateOfDeath?.ToString(WebConstants.DateDisplayFormat)),
            ("QTS held since", oldDetails.QtsDate?.ToString(WebConstants.DateDisplayFormat)),
            ("EYTS held since", oldDetails.EytsDate?.ToString(WebConstants.DateDisplayFormat)),
            ("QTLS held since", oldDetails.QtlsDate?.ToString(WebConstants.DateDisplayFormat)),
            ("Qualified teacher learning and skills status (QTLS)", oldDetails.QtlsStatus.GetDisplayName()),
            ("Induction status", oldDetails.InductionStatus?.GetDisplayName()),
            ("DQT induction status", oldDetails.DqtInductionStatus));
    }

    [Fact]
    public async Task WithNameChange_RendersCombinedCurrentAndPreviousNames()
    {
        // Arrange
        var details = CreatePersonDetails(firstName: "Jane", middleName: "Alice", lastName: "Smith");
        var oldDetails = CreatePersonDetails(firstName: "Janet", middleName: "Alice", lastName: "Smith");

        // Act
        var entry = await PublishPersonUpdatedInDqtEventAsync(PersonUpdatedInDqtEventChanges.FirstName, details, oldDetails);

        // Assert
        AssertTitle(entry, "Record updated");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Name", $"{details.FirstName} {details.MiddleName} {details.LastName}"));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("Name", $"{oldDetails.FirstName} {oldDetails.MiddleName} {oldDetails.LastName}"));
    }

    [Fact]
    public async Task WithSingleChange_RendersOnlyChangedField()
    {
        // Arrange
        var details = CreatePersonDetails(emailAddress: "new.email@example.com");
        var oldDetails = CreatePersonDetails(emailAddress: "old.email@example.com");

        // Act
        var entry = await PublishPersonUpdatedInDqtEventAsync(PersonUpdatedInDqtEventChanges.EmailAddress, details, oldDetails);

        // Assert
        AssertTitle(entry, "Record updated");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Email address", details.EmailAddress));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("Email address", oldDetails.EmailAddress));
    }

    private static IElement GetMainSummaryList(IHtmlElement entry) =>
        entry.QuerySelectorAll(".govuk-summary-list").First(sl => sl.Closest(".govuk-details") is null);

    private async Task<IHtmlElement> PublishPersonUpdatedInDqtEventAsync(
        PersonUpdatedInDqtEventChanges changes,
        EventModels.DqtPersonDetails details,
        EventModels.DqtPersonDetails oldDetails)
    {
        var person = await TestData.CreatePersonAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonUpdatingInDqt,
            SystemUser.Instance.UserId,
            changeReason: null,
            new PersonUpdatedInDqtEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = changes,
                Details = details,
                OldDetails = oldDetails
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private static EventModels.DqtPersonDetails CreatePersonDetails(
        string firstName = "Jane",
        string middleName = "Alice",
        string lastName = "Smith",
        DateOnly? dateOfBirth = null,
        string? emailAddress = "jane.smith@example.com",
        string? nationalInsuranceNumber = "QQ123456C",
        Gender? gender = Gender.Female,
        string? trn = "1234567",
        DateOnly? dateOfDeath = null,
        DateOnly? qtsDate = null,
        DateOnly? eytsDate = null,
        DateOnly? qtlsDate = null,
        QtlsStatus qtlsStatus = QtlsStatus.Active,
        InductionStatus? inductionStatus = InductionStatus.Passed,
        string? dqtInductionStatus = "Pass") =>
        new()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth ?? _dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender,
            Trn = trn,
            DateOfDeath = dateOfDeath,
            QtsDate = qtsDate,
            EytsDate = eytsDate,
            QtlsDate = qtlsDate,
            QtlsStatus = qtlsStatus,
            InductionStatus = inductionStatus,
            DqtInductionStatus = dqtInductionStatus
        };
}
