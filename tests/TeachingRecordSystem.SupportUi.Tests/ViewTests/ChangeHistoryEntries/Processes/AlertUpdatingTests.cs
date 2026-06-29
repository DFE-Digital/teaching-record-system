using AngleSharp.Dom;
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
        AssertTitle(entry, "Alert start date changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _newStartDate.ToString(WebConstants.DateDisplayFormat)));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)));
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertUpdatedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert start date changed");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task WithChangeReason_RendersCorrectly()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Another reason",
            Details = "Some reason details",
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
        AssertTitle(entry, "Alert start date changed");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for change", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason", changeReason.Reason);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason details", changeReason.Details);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
        changeReasonDetails.AssertSummaryListRowContentContains("Evidence", changeReason.EvidenceFile.Name);
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
        AssertTitle(entry, "Alert details changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("Details", _newDetails));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("Details", _oldDetails));
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
        AssertTitle(entry, "Alert link changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("External link", $"{_newExternalLink} (opens in new tab)"));

        // The old alert had no external link, so the previous data falls back to the empty placeholder
        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("External link", WebConstants.EmptyFallbackContent));
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
        AssertTitle(entry, "Alert re-opened");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("End date", WebConstants.EmptyFallbackContent));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("End date", _oldEndDate.ToString(WebConstants.DateDisplayFormat)));
    }

    [Fact]
    public async Task WithEndDateChange_WhenAlertClosed_RendersClosedAndOmitsPreviousData()
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
        AssertTitle(entry, "Alert closed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("End date", _newEndDate.ToString(WebConstants.DateDisplayFormat)));

        // Previous data is not rendered when an alert is closed
        Assert.Null(entry.GetElementByTestId("previous-data"));
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
        AssertTitle(entry, "Alert end date changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("End date", _newEndDate.ToString(WebConstants.DateDisplayFormat)));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("End date", _oldEndDate.ToString(WebConstants.DateDisplayFormat)));
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
        AssertTitle(entry, "Alert changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("DQT spent", true.ToString()));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("DQT spent", false.ToString()));
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
        AssertTitle(entry, "Alert changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("DQT sanction code", newSanctionCode.ToString()));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("DQT sanction code", oldSanctionCode.Value));
    }

    [Fact]
    public async Task WithMultipleChanges_RendersChanged()
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
        AssertTitle(entry, "Alert changed");

        GetMainSummaryList(entry).AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _newStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("Details", _newDetails));

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        previousData.AssertSummaryListHasRows(
            ("Start date", _oldStartDate.ToString(WebConstants.DateDisplayFormat)),
            ("Details", _oldDetails));
    }

    private static IElement GetMainSummaryList(IHtmlElement entry) =>
        entry.QuerySelectorAll(".govuk-summary-list").First(sl => sl.Closest(".govuk-details") is null);

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

        var baseAlert = EventModels.Alert.FromModel(person.Alerts.Single());
        var alert = configureNewAlert(baseAlert);
        var oldAlert = configureOldAlert(baseAlert);

        var processContext = new ProcessContext(
            ProcessType.AlertUpdating,
            TimeProvider.UtcNow,
            SystemUser.SystemUserId,
            changeReason);

        await WithEventPublisherAsync(publisher => publisher.PublishSingleEventAsync(
            new AlertUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = alert,
                OldAlert = oldAlert,
                Changes = changes
            },
            processContext));

        return await GetEntryHtmlAsync(processContext.ProcessId);
    }
}
