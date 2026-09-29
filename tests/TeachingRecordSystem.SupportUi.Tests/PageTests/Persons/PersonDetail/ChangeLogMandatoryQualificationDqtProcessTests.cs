using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogMandatoryQualificationDqtProcessTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Person_WithMandatoryQualificationDeactivatingInDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var mandatoryQualification = CreateMandatoryQualification();

        var process = await TestData.CreateProcessAsync(
            ProcessType.MandatoryQualificationDeactivatingInDqt,
            user.UserId,
            changeReason: null,
            new MandatoryQualificationDqtDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                MandatoryQualification = mandatoryQualification
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Mandatory qualification deactivated",
            user.Name,
            process.CreatedOn,
            [
                ("Training provider", "University of Manchester"),
                ("Specialism", MandatoryQualificationSpecialism.DeafEducation.GetTitle()),
                ("Start date", new DateOnly(2020, 1, 1).ToString(WebConstants.DateDisplayFormat)),
                ("Status", MandatoryQualificationStatus.Passed.GetTitle()),
                ("End date", new DateOnly(2021, 1, 1).ToString(WebConstants.DateDisplayFormat))
            ]);
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeactivatingInDqtProcessWithLegacyProvider_RendersExpectedProviderName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var legacyProvider = LegacyDataCache.Instance.GetAllMqEstablishments().SingleRandom();

        var mandatoryQualification = CreateMandatoryQualification() with
        {
            Provider = new EventModels.MandatoryQualificationProvider
            {
                MandatoryQualificationProviderId = null,
                Name = null,
                DqtMqEstablishmentValue = legacyProvider.Value,
                DqtMqEstablishmentName = legacyProvider.Name
            }
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.MandatoryQualificationDeactivatingInDqt,
            userId: null,
            changeReason: null,
            new MandatoryQualificationDqtDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                MandatoryQualification = mandatoryQualification
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var entry = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(entry);
        Assert.Equal(legacyProvider.Name, entry.GetElementByTestId("provider")?.TrimmedText());
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeactivatingInDqtProcessWithNoProvider_RendersNoneForProvider()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var mandatoryQualification = CreateMandatoryQualification() with { Provider = null };

        var process = await TestData.CreateProcessAsync(
            ProcessType.MandatoryQualificationDeactivatingInDqt,
            userId: null,
            changeReason: null,
            new MandatoryQualificationDqtDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                MandatoryQualification = mandatoryQualification
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var entry = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(entry);
        Assert.Equal("None", entry.GetElementByTestId("provider")?.TrimmedText());
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationImportingIntoDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.MandatoryQualificationImportingIntoDqt,
            user.UserId,
            changeReason: null,
            new MandatoryQualificationDqtImportedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                MandatoryQualification = CreateMandatoryQualification(),
                DqtState = 0
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Mandatory qualification imported",
            user.Name,
            process.CreatedOn);
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratingFromDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var process = await CreateMigratingProcessAsync(
            person.PersonId,
            user.UserId,
            CreateMandatoryQualification(),
            MandatoryQualificationMigratedEventChanges.None);

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Mandatory qualification migrated",
            user.Name,
            process.CreatedOn);
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratingFromDqtProcessWithNoChanges_DoesNotRenderPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateMigratingProcessAsync(
            person.PersonId,
            userId: null,
            CreateMandatoryQualification(),
            MandatoryQualificationMigratedEventChanges.None);

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var entry = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(entry);
        Assert.Null(entry.GetElementByTestId("previous-data"));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratingFromDqtProcessWithChangedProvider_RendersProviderRowInPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateMigratingProcessAsync(
            person.PersonId,
            userId: null,
            CreateMandatoryQualification(),
            MandatoryQualificationMigratedEventChanges.Provider);

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Mandatory qualification migrated",
            SystemUser.SystemUserName,
            process.CreatedOn,
            [],
            [("Training provider", "University of Manchester")]);
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratingFromDqtProcessWithChangedSpecialism_RendersSpecialismRowInPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateMigratingProcessAsync(
            person.PersonId,
            userId: null,
            CreateMandatoryQualification(),
            MandatoryQualificationMigratedEventChanges.Specialism);

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Mandatory qualification migrated",
            SystemUser.SystemUserName,
            process.CreatedOn,
            [],
            [("Specialism", MandatoryQualificationSpecialism.DeafEducation.GetTitle())]);
    }

    private Task<Process> CreateMigratingProcessAsync(
        Guid personId,
        Guid? userId,
        EventModels.MandatoryQualification mandatoryQualification,
        MandatoryQualificationMigratedEventChanges changes) =>
        TestData.CreateProcessAsync(
            ProcessType.MandatoryQualificationMigratingFromDqt,
            userId,
            changeReason: null,
            new MandatoryQualificationMigratedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                MandatoryQualification = mandatoryQualification,
                Changes = changes
            });

    private static EventModels.MandatoryQualification CreateMandatoryQualification() => new()
    {
        QualificationId = Guid.NewGuid(),
        Provider = new EventModels.MandatoryQualificationProvider
        {
            MandatoryQualificationProviderId = Guid.NewGuid(),
            Name = "University of Manchester"
        },
        Specialism = MandatoryQualificationSpecialism.DeafEducation,
        Status = MandatoryQualificationStatus.Passed,
        StartDate = new DateOnly(2020, 1, 1),
        EndDate = new DateOnly(2021, 1, 1)
    };
}
