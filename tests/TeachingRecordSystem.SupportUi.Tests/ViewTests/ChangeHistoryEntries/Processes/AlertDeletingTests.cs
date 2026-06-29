using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class AlertDeletingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertDeletedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert deleted");

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
        var entry = await PublishAlertDeletedEventAsync(alertType, changeReason: null);

        // Assert
        AssertTitle(entry, "Alert deleted");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task WithChangeReason_RendersCorrectly()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = null,
            Details = "Some deletion details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = new EventModels.File
            {
                FileId = Guid.NewGuid(),
                Name = "evidence.jpg"
            }
        };

        // Act
        var entry = await PublishAlertDeletedEventAsync(alertType, changeReason);

        // Assert
        AssertTitle(entry, "Alert deleted");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for deletion", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Deletion details", changeReason.Details);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
        changeReasonDetails.AssertSummaryListRowContentContains("Evidence", changeReason.EvidenceFile.Name);
    }

    private static readonly DateOnly _startDate = new(2020, 4, 9);

    private async Task<IHtmlElement> PublishAlertDeletedEventAsync(AlertType alertType, IChangeReasonInfo? changeReason)
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(_startDate)
                .WithEndDate(null)
                .WithDetails(Faker.Lorem.Paragraph())
                .WithExternalLink(Faker.Internet.Url())));

        var alert = person.Alerts.Single();

        await WithDbContextAsync(dbContext => dbContext.Alerts
            .Where(a => a.AlertId == alert.AlertId)
            .ExecuteUpdateAsync(e => e.SetProperty(a => a.DeletedOn, TimeProvider.UtcNow)));

        TimeProvider.Advance();

        var processContext = new ProcessContext(
            ProcessType.AlertDeleting,
            TimeProvider.UtcNow,
            SystemUser.SystemUserId,
            changeReason);

        await WithEventPublisherAsync(publisher => publisher.PublishSingleEventAsync(
            new AlertDeletedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Alert = EventModels.Alert.FromModel(alert)
            },
            processContext));

        return await GetEntryHtmlAsync(processContext.ProcessId);
    }
}
