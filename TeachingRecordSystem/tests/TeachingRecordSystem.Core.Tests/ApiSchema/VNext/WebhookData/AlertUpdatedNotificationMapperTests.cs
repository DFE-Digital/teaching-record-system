using TeachingRecordSystem.Core.ApiSchema.V3.VNext.WebhookData;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Tests.ApiSchema.VNext.WebhookData;

public class AlertUpdatedNotificationMapperTests(EventMapperFixture fixture) : EventMapperTestBase(fixture)
{
    [Fact]
    public Task MapEventAsync_AlertIsNotInternalOnly_ReturnsNotification() =>
        WithEventMapper<AlertUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .RandomOneExcept(a => a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();

            var @event = await DbFixture.WithDbContextAsync(async dbContext =>
            {
                dbContext.Alerts.Attach(alert);

                alert.Update(
                    a => a.Details = Faker.Lorem.Paragraph(),
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    changedBy: SystemUser.SystemUserId,
                    Clock.UtcNow,
                    out var updatedEvent);

                await dbContext.SaveChangesAsync();

                return updatedEvent;
            });

            // Act
            var notification = await mapper.MapEventAsync(@event!);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(person.Trn, notification.Trn);
            Assert.Equal(alert.AlertId, notification.Alert.AlertId);
            Assert.Equal(alert.AlertTypeId, notification.Alert.AlertType.AlertTypeId);
            Assert.Equal(alert.AlertType!.Name, notification.Alert.AlertType.Name);
            Assert.Equal(alert.AlertType.AlertCategoryId, notification.Alert.AlertType.AlertCategory.AlertCategoryId);
            Assert.Equal(alert.AlertType.AlertCategory!.Name, notification.Alert.AlertType.AlertCategory.Name);
            Assert.Equal(alert.Details, notification.Alert.Details);
            Assert.Equal(alert.StartDate, notification.Alert.StartDate);
            Assert.Equal(alert.EndDate, notification.Alert.EndDate);
        });

    [Fact]
    public Task MapEventAsync_AlertIsInternalOnly_ReturnsNull() =>
        WithEventMapper<AlertUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .RandomOneExcept(a => !a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();

            var @event = await DbFixture.WithDbContextAsync(async dbContext =>
            {
                dbContext.Alerts.Attach(alert);

                alert.Update(
                    a => a.Details = Faker.Lorem.Paragraph(),
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    changedBy: SystemUser.SystemUserId,
                    Clock.UtcNow,
                    out var updatedEvent);

                await dbContext.SaveChangesAsync();

                return updatedEvent;
            });

            // Act
            var notification = await mapper.MapEventAsync(@event!);

            // Assert
            Assert.Null(notification);
        });

    [Fact]
    public Task MapEventAsync_OnlyLinkIsChanged_ReturnsNull() =>
        WithEventMapper<AlertUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .RandomOneExcept(a => !a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();

            var @event = await DbFixture.WithDbContextAsync(async dbContext =>
            {
                dbContext.Alerts.Attach(alert);

                alert.Update(
                    a => a.ExternalLink = Faker.Internet.Url(),
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    changedBy: SystemUser.SystemUserId,
                    Clock.UtcNow,
                    out var updatedEvent);

                await dbContext.SaveChangesAsync();

                return updatedEvent;
            });

            // Act
            var notification = await mapper.MapEventAsync(@event!);

            // Assert
            Assert.Null(notification);
        });
}
