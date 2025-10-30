using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Xunit.Sdk;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeHistoryTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task Get_WithPersonUpdatingInDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var qtlsDate = new DateOnly(2024, 4, 1);
        var qtlsStatus = QtlsStatus.Active;

        var person = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber()
            .WithEmailAddress()
            .WithGender()
            .WithQts()
            .WithQtls(qtlsDate)
            .WithQtlsStatus(qtlsStatus)
            .WithEyts()
            .WithInductionStatus(InductionStatus.Passed));

        var @event = new PersonUpdatedInDqtEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Changes = PersonUpdatedInDqtEventChanges.All,
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
            DqtInductionStatus = person.Person.InductionStatus.ToDqtInductionStatus(out _),
            QtlsDate = qtlsDate,
            QtlsStatus = qtlsStatus
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonUpdatingInDqt, user.UserId, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record updated",
            user.Name,
            process.CreatedOn,
            ("TRN", person.Trn),
            ("Name", $"{person.FirstName} {person.MiddleName} {person.LastName}"),
            ("Date of birth", person.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("Email address", person.EmailAddress),
            ("National Insurance number", person.NationalInsuranceNumber),
            ("Gender", person.Gender?.GetDisplayName()),
            ("Date of death", person.Person.DateOfDeath?.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("QTS held since", person.QtsDate?.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("EYTS held since", person.EytsDate?.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("QTLS held since", qtlsDate.ToString(UiDefaults.DateOnlyDisplayFormat)),
            ("Qualified teacher learning and skills status (QTLS)", qtlsStatus.GetDisplayName()),
            ("Induction status", person.Person.InductionStatus.GetDisplayName()),
            ("DQT induction status", person.Person.InductionStatus.ToDqtInductionStatus(out _)));
    }

    [Fact]
    public async Task Get_WithPersonDeactivatingInDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var @event = new PersonDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MergedWithPersonId = null
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonDeactivatingInDqt, user.UserId, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record deactivated",
            user.Name,
            process.CreatedOn);
    }

    [Fact]
    public async Task Get_WithPersonReactivatingInDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var @event = new PersonReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId
        };

        var user = SystemUser.Instance;
        var process = await TestData.CreateProcessAsync(ProcessType.PersonReactivatingInDqt, user.UserId, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record reactivated",
            user.Name,
            process.CreatedOn);
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
        if (changeHistoryItem is null)
        {
            throw new XunitException($"Element with data-process-id=\"{processId}\" not found.");
        }

        var title = changeHistoryItem.GetElementsByClassName("moj-timeline__title").SingleOrDefault();
        Assert.Equal(expectedTitle, title?.TrimmedText());

        var date = changeHistoryItem.GetElementsByClassName("moj-timeline__date").SingleOrDefault();
        var expectedDateBlock = $"By {expectedUserName} on {expectedTimestamp:d MMMMM yyyy 'at' h:mm tt}";
        Assert.Equal(expectedDateBlock, date?.TrimmedText().ReplaceLineEndings(" "), ignoreAllWhiteSpace: true);

        if (expectedSummaryListRows.Length > 0)
        {
            var description = changeHistoryItem.GetElementsByClassName("moj-timeline__description").SingleOrDefault()?.FirstElementChild;
            if (description is null)
            {
                throw new XunitException("Element with class=\"moj-timeline__description\" not found.");
            }

            description.AssertSummaryListHasRows(
                expectedSummaryListRows.Select(e => e with { Value = e.Value ?? UiDefaults.EmptyDisplayContent }).ToArray());
        }
    }
}
