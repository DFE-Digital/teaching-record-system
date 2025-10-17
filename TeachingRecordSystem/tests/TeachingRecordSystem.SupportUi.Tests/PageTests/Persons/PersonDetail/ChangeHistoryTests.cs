using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeHistoryTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_WithPersonCreatingInDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());

        var @event = new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = person.EmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            CreateReason = null,
            CreateReasonDetail = null,
            EvidenceFile = null,
            TrnRequestMetadata = null
        };

        var user = await TestData.CreateUserAsync();
        var process = await TestData.CreateProcessAsync(ProcessType.PersonCreatingInDqt, user.UserId, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record created",
            user.Name,
            process.CreatedOn,
            ("Name", $"{person.FirstName} {person.MiddleName} {person.LastName}"),
            ("Date of birth", person.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("Email address", person.EmailAddress),
            ("National Insurance number", person.NationalInsuranceNumber),
            ("Gender", person.Gender?.GetDisplayName()));
    }

    [Test]
    public async Task Get_WithPersonImportedIntoDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber()
            .WithEmailAddress()
            .WithGender()
            .WithQts()
            .WithEyts()
            .WithInductionStatus(InductionStatus.Passed));

        var @event = new PersonImportedIntoDqtEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = person.EmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            Trn = person.Trn!,
            DateOfDeath = null,
            QtsDate = person.QtsDate,
            EytsDate = person.EytsDate,
            InductionStatus = person.Person.InductionStatus,
            DqtInductionStatus = person.Person.InductionStatus.ToDqtInductionStatus(out _)
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonImportingIntoDqt, user.UserId, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record created",
            user.Name,
            process.CreatedOn,
            ("TRN", person.Trn),
            ("Name", $"{person.FirstName} {person.MiddleName} {person.LastName}"),
            ("Date of birth", person.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("Email address", person.EmailAddress),
            ("National Insurance number", person.NationalInsuranceNumber),
            ("Gender", person.Gender?.GetDisplayName()),
            ("QTS held since", person.QtsDate?.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("EYTS held since", person.EytsDate?.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("Induction status", person.Person.InductionStatus.GetDisplayName()),
            ("DQT induction status", person.Person.InductionStatus.ToDqtInductionStatus(out _)));
    }
}

file static class Extensions
{
    public static void AssertHasChangeHistoryEntry(
        this IHtmlDocument doc,
        Guid processId,
        string expectedTitle,
        string expectedUserName,
        DateTime expectedTimestamp,
        params (string Key, string? Value)[] expectedSummaryListRows)
    {
        var changeHistoryItem = doc.GetElementByDataAttribute("data-process-id", processId.ToString());
        Assert.NotNull(changeHistoryItem);

        var title = changeHistoryItem.GetElementsByClassName("moj-timeline__title").SingleOrDefault();
        Assert.Equal(expectedTitle, title?.TrimmedText());

        var date = changeHistoryItem.GetElementsByClassName("moj-timeline__date").SingleOrDefault();
        var expectedDateBlock = $"By {expectedUserName} on {expectedTimestamp:d MMMMM yyyy 'at' h:mm tt}";
        Assert.Equal(expectedDateBlock, date?.TrimmedText().ReplaceNewLines(), ignoreAllWhiteSpace: true);

        var description = changeHistoryItem.GetElementsByClassName("moj-timeline__description").SingleOrDefault()?.FirstElementChild;
        Assert.NotNull(description);
        description.AssertSummaryListHasRows(expectedSummaryListRows);
    }
}
