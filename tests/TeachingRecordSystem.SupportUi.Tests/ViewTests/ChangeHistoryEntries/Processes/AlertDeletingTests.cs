using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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

    private async Task<IHtmlElement> PublishAlertDeletedEventAsync(AlertType alertType)
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
            changeReason: null,
            events: new AlertDeletedEvent { EventId = Guid.NewGuid(), PersonId = person.PersonId, Alert = EventModels.Alert.FromModel(alert) });

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}
