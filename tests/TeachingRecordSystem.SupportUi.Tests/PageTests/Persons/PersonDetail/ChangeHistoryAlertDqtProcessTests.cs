using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public partial class ChangeHistoryTests
{
    [Fact]
    public async Task Get_WithAlertImportingIntoDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await GetT1AlertTypeAsync();
        var startDate = new DateOnly(2015, 6, 3);
        var alert = CreateDqtAlert(alertType.AlertTypeId, startDate);

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertImportingIntoDqt,
            userId: null,
            changeReason: null,
            new AlertDqtImportedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = alert,
                DqtState = 0
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert imported",
            SystemUser.SystemUserName,
            process.CreatedOn,
            [
                ("DQT sanction code", "T1"),
                ("DQT sanction name", T1SanctionName),
                ("Start date", startDate.ToString(WebConstants.DateDisplayFormat))
            ]);
    }

    [Fact]
    public async Task Get_WithAlertReactivatingInDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await GetT1AlertTypeAsync();
        var startDate = new DateOnly(2016, 2, 29);
        var alert = CreateDqtAlert(alertType.AlertTypeId, startDate);

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertReactivatingInDqt,
            userId: null,
            changeReason: null,
            new AlertDqtReactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = alert
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert reactivated",
            SystemUser.SystemUserName,
            process.CreatedOn,
            [
                ("DQT sanction code", "T1"),
                ("DQT sanction name", T1SanctionName),
                ("Start date", startDate.ToString(WebConstants.DateDisplayFormat))
            ]);
    }

    [Fact]
    public async Task Get_WithAlertDeactivatingInDqtProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await GetT1AlertTypeAsync();
        var startDate = new DateOnly(2017, 11, 8);
        var endDate = new DateOnly(2018, 1, 31);
        var alert = CreateDqtAlert(alertType.AlertTypeId, startDate) with { EndDate = endDate, DqtSpent = false };

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertDeactivatingInDqt,
            userId: null,
            changeReason: null,
            new AlertDqtDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = alert
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert deactivated",
            SystemUser.SystemUserName,
            process.CreatedOn,
            [
                ("DQT sanction code", "T1"),
                ("DQT sanction name", T1SanctionName),
                ("Start date", startDate.ToString(WebConstants.DateDisplayFormat)),
                ("Details", "Some alert details"),
                ("External link", null),
                ("End date", endDate.ToString(WebConstants.DateDisplayFormat)),
                ("DQT spent", "False")
            ]);
    }

    [Fact]
    public async Task Get_WithAlertMigratingFromDqtProcess_RendersExpectedEntryWithPreviousData()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await GetT1AlertTypeAsync();
        var startDate = new DateOnly(2019, 4, 17);
        var alert = CreateDqtAlert(alertType.AlertTypeId, startDate) with { DqtSanctionCode = null };
        var oldAlert = CreateDqtAlert(alertType.AlertTypeId, startDate) with { DqtSpent = true };

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertMigratingFromDqt,
            userId: null,
            changeReason: null,
            new AlertMigratedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = alert,
                OldAlert = oldAlert
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert migrated",
            SystemUser.SystemUserName,
            process.CreatedOn,
            [
                ("Alert type", alertType.Name),
                ("Start date", startDate.ToString(WebConstants.DateDisplayFormat))
            ],
            [
                ("DQT sanction code", "T1"),
                ("DQT sanction name", T1SanctionName),
                ("DQT spent", "True")
            ]);
    }

    [Theory]
    [InlineData(false, UserRoles.Viewer, true)]
    [InlineData(true, UserRoles.Viewer, false)]
    [InlineData(true, UserRoles.AlertsManagerTraDbs, true)]
    public async Task Get_WithAlertImportingIntoDqtProcess_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : await GetNonDbsAlertTypeAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertImportingIntoDqt,
            userId: null,
            changeReason: null,
            new AlertDqtImportedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = CreateDqtAlert(alertType.AlertTypeId, new DateOnly(2020, 1, 1)) with { DqtSanctionCode = null },
                DqtState = 0
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var entry = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());

        if (shouldDisplay)
        {
            Assert.NotNull(entry);
        }
        else
        {
            Assert.Null(entry);
        }
    }

    private const string T1SanctionName = "Prohibition by the Secretary of State - misconduct";

    private static readonly Guid _t1AlertTypeId = Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed");

    private Task<AlertType> GetT1AlertTypeAsync() => TestData.ReferenceDataCache.GetAlertTypeByIdAsync(_t1AlertTypeId);

    private async Task<AlertType> GetNonDbsAlertTypeAsync() =>
        (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);

    private static EventModels.Alert CreateDqtAlert(Guid alertTypeId, DateOnly startDate) => new()
    {
        AlertId = Guid.NewGuid(),
        AlertTypeId = alertTypeId,
        Details = "Some alert details",
        ExternalLink = null,
        StartDate = startDate,
        EndDate = null,
        DqtSanctionCode = new EventModels.AlertDqtSanctionCode { Value = "T1", Name = T1SanctionName }
    };
}
