using TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;

namespace TeachingRecordSystem.Core.Tests.ApiSchema.V20250804.WebhookData;

public class AlertCreatedNotificationMapperTests(EventMapperFixture fixture) : EventMapperTestBase(fixture)
{
    [Fact]
    public Task MapEventAsync_AlertIsNotInternalOnly_ReturnsNotification() =>
        WithEventMapper<AlertCreatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .RandomOneExcept(a => a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();
            var @event = person.Events.OfType<AlertCreatedEvent>().Single();

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(person.Trn, notification.Trn);
            Assert.Equal(alert.AlertId, notification.Alert.AlertId);
            Assert.Equal(alert.AlertTypeId, notification.Alert.AlertType.AlertTypeId);
            Assert.Equal(alert.AlertType!.Name, notification.Alert.AlertType.Name);
            Assert.Equal(alert.AlertType.AlertCategoryId, notification.Alert.AlertType.AlertCategory.AlertCategoryId);
            Assert.Equal(alert.AlertType!.AlertCategory!.Name, notification.Alert.AlertType.AlertCategory.Name);
            Assert.Equal(alert.Details, notification.Alert.Details);
            Assert.Equal(alert.StartDate, notification.Alert.StartDate);
            Assert.Equal(alert.EndDate, notification.Alert.EndDate);
        });

    [Fact]
    public Task MapEventAsync_AlertIsInternalOnly_ReturnsNull() =>
        WithEventMapper<AlertCreatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .RandomOneExcept(a => !a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();
            var @event = person.Events.OfType<AlertCreatedEvent>().Single();

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.Null(notification);
        });
}
