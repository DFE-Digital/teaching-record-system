using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class AlertAddingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    private static readonly DateOnly _startDate = new(2020, 4, 9);

    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertCreatedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert added");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Alert added for", bodyText);
        Assert.Contains(alertType.Name, bodyText);
        Assert.Contains(_startDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertCreatedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert added");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task WithEmptyChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = null,
            Details = null,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        // Act
        var entry = await PublishAlertCreatedEventAsync(alertType, changeReason);

        // Assert
        AssertTitle(entry, "Alert added");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Theory]
    [InlineData("Another reason", "Some reason details", "Another reason: Some reason details")]
    [InlineData("Routine notification from stakeholder", null, "Routine notification from stakeholder")]
    [InlineData("Identified during data reconciliation with stakeholder", "", "Identified during data reconciliation with stakeholder")]
    public async Task WithChangeReason_RendersCorrectly(string reason, string? details, string expectedReasonDetails)
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = reason,
            Details = details,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        // Act
        var entry = await PublishAlertCreatedEventAsync(alertType, changeReason);

        // Assert
        AssertTitle(entry, "Alert added");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for adding alert", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonDetails);
    }

    [Fact]
    public async Task WithDqtSanctionCode_RendersDqtSanctionName()
    {
        // Arrange
        var dqtSanctionCode = "T1";
        var dqtSanctionName = "Test Sanction";

        // Act
        var entry = await PublishAlertCreatedEventWithDqtSanctionAsync(dqtSanctionCode, dqtSanctionName, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert added");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Alert added for", bodyText);
        Assert.Contains(dqtSanctionName, bodyText);
        Assert.Contains(_startDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    private async Task<IHtmlElement> PublishAlertCreatedEventAsync(AlertType alertType, IChangeReasonInfo? changeReason)
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(_startDate)
                .WithEndDate(null)  // UI doesn't allow creating a closed alert
                .WithDetails(Faker.Lorem.Paragraph())
                .WithExternalLink(Faker.Internet.Url())));

        var alert = person.Alerts!.Single();

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertCreating,
            changeReason: changeReason,
            events: new AlertCreatedEvent { EventId = Guid.NewGuid(), PersonId = person.PersonId, Alert = EventModels.Alert.FromModel(alert) });

        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private async Task<IHtmlElement> PublishAlertCreatedEventWithDqtSanctionAsync(string dqtSanctionCode, string dqtSanctionName, IChangeReasonInfo? changeReason)
    {
        var person = await TestData.CreatePersonAsync();

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = null,  // No alert type so DQT sanction name will be displayed
            Details = Faker.Lorem.Paragraph(),
            ExternalLink = Faker.Internet.Url(),
            StartDate = _startDate,
            EndDate = null,
            DqtSanctionCode = new EventModels.AlertDqtSanctionCode { Value = dqtSanctionCode, Name = dqtSanctionName }
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertCreating,
            changeReason: changeReason,
            events: new AlertCreatedEvent { EventId = Guid.NewGuid(), PersonId = person.PersonId, Alert = alert });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
