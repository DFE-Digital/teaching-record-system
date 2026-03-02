using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public partial class ChangeHistoryTests
{
    [Fact]
    public async Task Get_WithAlertCreatingProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed"));
        var startDate = new DateOnly(2024, 1, 15);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType.AlertTypeId,
            Details = "Test alert details",
            ExternalLink = null,
            StartDate = startDate,
            EndDate = null,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        var @event = new AlertCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Alert = alert
        };

        var user = await TestData.CreateUserAsync();
        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Safeguarding concern",
            Details = "Details about the alert",
            EvidenceFile = null
        };
        var process = await TestData.CreateProcessAsync(ProcessType.AlertCreating, user.UserId, changeReason, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert added",
            user.Name,
            process.CreatedOn,
            [
                ("Alert type", alertType.Name),
                ("Start date", startDate.ToString(WebConstants.DateOnlyDisplayFormat))
            ]);
    }

    [Theory]
    [InlineData(false, UserRoles.Viewer, true)]
    [InlineData(false, UserRoles.RecordManager, true)]
    [InlineData(false, UserRoles.AlertsManagerTra, true)]
    [InlineData(false, UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(false, UserRoles.Administrator, true)]
    [InlineData(true, UserRoles.Viewer, false)]
    [InlineData(true, UserRoles.RecordManager, false)]
    [InlineData(true, UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(true, UserRoles.AlertsManagerTra, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Get_WithAlertCreatingProcess_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);
        var startDate = new DateOnly(2024, 1, 15);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType.AlertTypeId,
            Details = "Test alert details",
            ExternalLink = null,
            StartDate = startDate,
            EndDate = null,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        var @event = new AlertCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Alert = alert
        };

        var user = await TestData.CreateUserAsync();
        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Safeguarding concern",
            Details = "Details about the alert",
            EvidenceFile = null
        };
        var process = await TestData.CreateProcessAsync(ProcessType.AlertCreating, user.UserId, changeReason, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());

        if (shouldDisplay)
        {
            Assert.NotNull(item);
        }
        else
        {
            Assert.Null(item);
        }
    }

    [Fact]
    public async Task Get_WithAlertUpdatingProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed"));
        var startDate = new DateOnly(2024, 1, 15);
        var newStartDate = new DateOnly(2024, 1, 20);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType.AlertTypeId,
            Details = "Updated alert details",
            ExternalLink = "https://example.com",
            StartDate = newStartDate,
            EndDate = null,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        var oldAlert = alert with
        {
            Details = "Original alert details",
            ExternalLink = null,
            StartDate = startDate
        };

        var @event = new AlertUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Alert = alert,
            OldAlert = oldAlert,
            Changes = AlertUpdatedEventChanges.Details | AlertUpdatedEventChanges.ExternalLink | AlertUpdatedEventChanges.StartDate
        };

        var user = await TestData.CreateUserAsync();
        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Update required",
            Details = "Updating alert information",
            EvidenceFile = null
        };
        var process = await TestData.CreateProcessAsync(ProcessType.AlertUpdating, user.UserId, changeReason, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert changed",
            user.Name,
            process.CreatedOn,
            [
                ("Alert type", alertType.Name),
                ("Start date", newStartDate.ToString(WebConstants.DateOnlyDisplayFormat)),
                ("Details", "Updated alert details"),
                ("External link", $"https://example.com (opens in new tab)")
            ],
            [
                ("Start date", startDate.ToString(WebConstants.DateOnlyDisplayFormat)),
                ("Details", "Original alert details"),
                ("External link", WebConstants.EmptyFallbackContent)
            ]);
    }

    [Theory]
    [InlineData(false, UserRoles.Viewer, true)]
    [InlineData(false, UserRoles.RecordManager, true)]
    [InlineData(false, UserRoles.AlertsManagerTra, true)]
    [InlineData(false, UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(false, UserRoles.Administrator, true)]
    [InlineData(true, UserRoles.Viewer, false)]
    [InlineData(true, UserRoles.RecordManager, false)]
    [InlineData(true, UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(true, UserRoles.AlertsManagerTra, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Get_WithAlertUpdatingProcess_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);
        var startDate = new DateOnly(2024, 1, 15);
        var newStartDate = new DateOnly(2024, 1, 20);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType.AlertTypeId,
            Details = "Updated alert details",
            ExternalLink = "https://example.com",
            StartDate = newStartDate,
            EndDate = null,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        var oldAlert = alert with
        {
            Details = "Original alert details",
            ExternalLink = null,
            StartDate = startDate
        };

        var @event = new AlertUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Alert = alert,
            OldAlert = oldAlert,
            Changes = AlertUpdatedEventChanges.Details | AlertUpdatedEventChanges.ExternalLink | AlertUpdatedEventChanges.StartDate
        };

        var user = await TestData.CreateUserAsync();
        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Update required",
            Details = "Updating alert information",
            EvidenceFile = null
        };
        var process = await TestData.CreateProcessAsync(ProcessType.AlertUpdating, user.UserId, changeReason, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());

        if (shouldDisplay)
        {
            Assert.NotNull(item);
        }
        else
        {
            Assert.Null(item);
        }
    }

    [Fact]
    public async Task Get_WithAlertDeletingProcess_RendersExpectedEntry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = new DateOnly(2024, 1, 15);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType.AlertTypeId,
            Details = "Test alert details",
            ExternalLink = null,
            StartDate = startDate,
            EndDate = null,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        var @event = new AlertDeletedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Alert = alert
        };

        var user = await TestData.CreateUserAsync();
        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = null,
            Details = "Alert no longer applicable",
            EvidenceFile = null
        };
        var process = await TestData.CreateProcessAsync(ProcessType.AlertDeleting, user.UserId, changeReason, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Alert deleted",
            user.Name,
            process.CreatedOn,
            [
                ("Alert type", alertType.Name),
                ("Start date", startDate.ToString(WebConstants.DateOnlyDisplayFormat))
            ]);
    }

    [Theory]
    [InlineData(false, UserRoles.Viewer, true)]
    [InlineData(false, UserRoles.RecordManager, true)]
    [InlineData(false, UserRoles.AlertsManagerTra, true)]
    [InlineData(false, UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(false, UserRoles.Administrator, true)]
    [InlineData(true, UserRoles.Viewer, false)]
    [InlineData(true, UserRoles.RecordManager, false)]
    [InlineData(true, UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(true, UserRoles.AlertsManagerTra, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Get_WithAlertDeletingProcess_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);
        var startDate = new DateOnly(2024, 1, 15);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType.AlertTypeId,
            Details = "Test alert details",
            ExternalLink = null,
            StartDate = startDate,
            EndDate = null,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        var @event = new AlertDeletedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Alert = alert
        };

        var user = await TestData.CreateUserAsync();
        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = null,
            Details = "Alert no longer applicable",
            EvidenceFile = null
        };
        var process = await TestData.CreateProcessAsync(ProcessType.AlertDeleting, user.UserId, changeReason, @event);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());

        if (shouldDisplay)
        {
            Assert.NotNull(item);
        }
        else
        {
            Assert.Null(item);
        }
    }
}
