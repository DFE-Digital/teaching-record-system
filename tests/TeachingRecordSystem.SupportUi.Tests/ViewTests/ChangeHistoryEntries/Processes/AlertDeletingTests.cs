using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class AlertDeletingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    private static readonly DateOnly _startDate = new(2020, 4, 9);

    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var alertType = (await ReferenceDataCache.GetAlertTypesAsync()).SingleRandom();

        // Act
        var entry = await PublishAlertDeletedEventAsync(alertType);

        // Assert
        AssertTitle(entry, "Alert deleted");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Alert deleted for", bodyText);
        Assert.Contains(alertType.Name, bodyText);
        Assert.Contains(_startDate.ToString(WebConstants.DateDisplayFormat), bodyText);
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
        var entry = await PublishAlertDeletedEventAsync(alertType, changeReason);

        // Assert
        AssertTitle(entry, "Alert deleted");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for deleting alert", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonDetails);
        changeReasonDetails.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
    }

    private Task<IHtmlElement> PublishAlertDeletedEventAsync(AlertType alertType, IChangeReasonInfo? changeReason = null)
    {
        return PublishAlertDeletedEventInternalAsync(alertType, changeReason);
    }

    private async Task<IHtmlElement> PublishAlertDeletedEventInternalAsync(AlertType alertType, IChangeReasonInfo? changeReason)
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a
                .WithAlertTypeId(alertType.AlertTypeId)
                .WithStartDate(_startDate)
                .WithEndDate(null)
                .WithDetails(Faker.Lorem.Paragraph())
                .WithExternalLink(Faker.Internet.Url())));

        var alert = person.Alerts!.Single();

        await WithDbContextAsync(dbContext => dbContext.Alerts
            .Where(a => a.AlertId == alert.AlertId)
            .ExecuteUpdateAsync(e => e.SetProperty(a => a.DeletedOn, TimeProvider.UtcNow)));

        TimeProvider.Advance();

        var process = await TestData.CreateProcessAsync(
            ProcessType.AlertDeleting,
            changeReason: changeReason,
            events: new AlertDeletedEvent { EventId = Guid.NewGuid(), PersonId = person.PersonId, Alert = EventModels.Alert.FromModel(alert) });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
