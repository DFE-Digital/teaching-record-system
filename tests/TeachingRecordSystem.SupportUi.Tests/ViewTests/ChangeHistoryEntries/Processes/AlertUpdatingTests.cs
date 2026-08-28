using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class AlertUpdatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    private static readonly DateOnly _oldStartDate = new(2020, 4, 9);
    private static readonly DateOnly _newStartDate = new(2021, 6, 14);
    private static readonly DateOnly _oldEndDate = new(2022, 1, 1);
    private static readonly DateOnly _newEndDate = new(2023, 2, 2);
    private const string _oldDetails = "Old details";
    private const string _newDetails = "New details";
    private const string _oldExternalLink = "https://old.example.com";
    private const string _newExternalLink = "https://new.example.com";

    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Start date changed from", bodyText);
        Assert.Contains(_oldStartDate.ToString(WebConstants.DateDisplayFormat), bodyText);
        Assert.Contains(_newStartDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert updated");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Theory]
    [InlineData("Another reason", "Some reason details", "Another reason: Some reason details")]
    [InlineData("Routine notification from stakeholder", null, "Routine notification from stakeholder")]
    public async Task WithChangeReason_RendersCorrectly(string reason, string? details, string expectedReasonDetails)
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = reason,
            Details = details,
            AdditionalInformation = "Some additional information",
            EvidenceFile = new EventModels.File
            {
                FileId = Guid.NewGuid(),
                Name = "evidence.jpg"
            }
        };

        // Act
        var entry = await PublishAlertUpdatedEventAsync(alertType, changeReason);

        // Assert
        AssertTitle(entry, "Alert updated");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for change", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonDetails);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
    }

    [Fact]
    public async Task WithDetailsChange_RendersDetailsChange()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.Details,
            a => a with { Details = _newDetails },
            a => a with { Details = _oldDetails });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Details changed from", bodyText);
        Assert.Contains(_oldDetails, bodyText);
        Assert.Contains(_newDetails, bodyText);
    }

    [Fact]
    public async Task WithExternalLinkChange_RendersLinkChange()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.ExternalLink,
            a => a with { ExternalLink = _newExternalLink },
            a => a with { ExternalLink = null });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("External link changed from", bodyText);
        Assert.Contains("(none)", bodyText);
        Assert.Contains(_newExternalLink, bodyText);
    }

    [Fact]
    public async Task WithEndDateChange_WhenAlertReopened_RendersReopened()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.EndDate,
            a => a with { EndDate = null },
            a => a with { EndDate = _oldEndDate });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Alert re-opened", bodyText);
        Assert.Contains(_oldEndDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task WithEndDateChange_WhenAlertClosed_RendersClosed()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.EndDate,
            a => a with { EndDate = _newEndDate },
            a => a with { EndDate = null });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Alert closed", bodyText);
        Assert.Contains(_newEndDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task WithEndDateChange_WhenEndDateChanged_RendersEndDateChanged()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.EndDate,
            a => a with { EndDate = _newEndDate },
            a => a with { EndDate = _oldEndDate });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("End date changed from", bodyText);
        Assert.Contains(_oldEndDate.ToString(WebConstants.DateDisplayFormat), bodyText);
        Assert.Contains(_newEndDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task WithDqtSpentChange_RendersDqtSpent()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.DqtSpent,
            a => a with { DqtSpent = true },
            a => a with { DqtSpent = false });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("DQT spent changed from", bodyText);
        Assert.Contains("False", bodyText);
        Assert.Contains("True", bodyText);
    }

    [Fact]
    public async Task WithDqtSanctionCodeChange_RendersDqtSanctionCode()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        var newSanctionCode = new EventModels.AlertDqtSanctionCode { Name = "New name", Value = "B1" };
        var oldSanctionCode = new EventModels.AlertDqtSanctionCode { Name = "Old name", Value = "A1" };

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.DqtSanctionCode,
            a => a with { DqtSanctionCode = newSanctionCode },
            a => a with { DqtSanctionCode = oldSanctionCode });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("DQT sanction code changed from", bodyText);
        Assert.Contains("Old name", bodyText);
        Assert.Contains("New name", bodyText);
    }

    [Fact]
    public async Task WithMultipleChanges_RendersAllChanges()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.StartDate | AlertUpdatedEventChanges.Details,
            a => a with { StartDate = _newStartDate, Details = _newDetails },
            a => a with { StartDate = _oldStartDate, Details = _oldDetails });

        // Assert
        AssertTitle(entry, "Alert updated");

        var bodyTexts = entry.GetElementsByClassName("govuk-body").Select(e => e.TrimmedText()).ToList();
        Assert.Contains(bodyTexts, t => t.Contains("Start date changed from"));
        Assert.Contains(bodyTexts, t => t.Contains("Details changed from"));
    }

    private Task<IHtmlElement> PublishAlertUpdatedEventAsync(AlertType alertType, IChangeReasonInfo? changeReason = null) =>
        PublishAlertUpdatedEventAsync(
            alertType,
            AlertUpdatedEventChanges.StartDate,
            a => a with { StartDate = _newStartDate },
            a => a with { StartDate = _oldStartDate },
            changeReason);

    private async Task<IHtmlElement> PublishAlertUpdatedEventAsync(
        AlertType alertType,
        AlertUpdatedEventChanges changes,
        Func<EventModels.Alert, EventModels.Alert> configureNewAlert,
        Func<EventModels.Alert, EventModels.Alert> configureOldAlert,
        IChangeReasonInfo? changeReason = null)
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(_oldStartDate)
                .WithEndDate(null)
                .WithDetails(_oldDetails)
                .WithExternalLink(_oldExternalLink)));

        var baseAlert = EventModels.Alert.FromModel(person.Alerts!.Single());
        var alert = configureNewAlert(baseAlert);
        var oldAlert = configureOldAlert(baseAlert);

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertUpdating,
            changeReason: changeReason,
            events: new AlertUpdatedEvent { EventId = Guid.NewGuid(), PersonId = person.PersonId, Alert = alert, OldAlert = oldAlert, Changes = changes });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
