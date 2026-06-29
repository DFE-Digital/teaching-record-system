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

        entry.AssertSummaryListHasRows(
            ("Alert type", alertType.Name),
            ("Start date", _startDate.ToString(WebConstants.DateDisplayFormat)));
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
        var entry = await PublishAlertCreatedEventAsync(alertType, changeReason);

        // Assert
        AssertTitle(entry, "Alert added");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for adding alert", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason", changeReason.Reason);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason details", changeReason.Details);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
        changeReasonDetails.AssertSummaryListRowContentContains("Evidence", changeReason.EvidenceFile.Name);
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

        var alert = person.Alerts.Single();

        var processContext = new ProcessContext(
            ProcessType.AlertCreating,
            TimeProvider.UtcNow,
            SystemUser.SystemUserId,
            changeReason);

        await WithEventPublisherAsync(publisher => publisher.PublishSingleEventAsync(
            new AlertCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = EventModels.Alert.FromModel(alert)
            },
            processContext));

        return await GetEntryHtmlAsync(processContext.ProcessId);
    }
}
