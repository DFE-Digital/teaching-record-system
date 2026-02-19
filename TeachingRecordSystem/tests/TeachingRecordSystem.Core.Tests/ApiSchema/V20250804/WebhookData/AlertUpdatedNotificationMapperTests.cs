using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.ApiSchema.V20250804.WebhookData;

public class AlertUpdatedNotificationMapperTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    private ReferenceDataCache ReferenceDataCache => Services.GetRequiredService<ReferenceDataCache>();

    [Fact]
    public Task MapEventAsync_AlertIsNotInternalOnly_ReturnsNotification() =>
        WithServiceAsync<AlertUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .SingleRandom(a => !a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();

            var @event = await WithDbContextAsync(async dbContext =>
            {
                dbContext.Alerts.Attach(alert);

                var oldDetails = alert.Details;
                var newDetails = Faker.Lorem.Paragraph();
                alert.Details = newDetails;
                alert.UpdatedOn = Clock.UtcNow;

                var updatedEvent = new LegacyEvents.AlertUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = Clock.UtcNow,
                    RaisedBy = SystemUser.SystemUserId,
                    PersonId = alert.PersonId,
                    Alert = EventModels.Alert.FromModel(alert),
                    OldAlert = new EventModels.Alert
                    {
                        AlertId = alert.AlertId,
                        AlertTypeId = alert.AlertTypeId,
                        Details = oldDetails,
                        ExternalLink = alert.ExternalLink,
                        StartDate = alert.StartDate,
                        EndDate = alert.EndDate
                    },
                    ChangeReason = null,
                    ChangeReasonDetail = null,
                    EvidenceFile = null,
                    Changes = LegacyEvents.AlertUpdatedEventChanges.Details
                };

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
        WithServiceAsync<AlertUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .SingleRandom(a => a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();

            var @event = await WithDbContextAsync(async dbContext =>
            {
                dbContext.Alerts.Attach(alert);

                var oldDetails = alert.Details;
                var newDetails = Faker.Lorem.Paragraph();
                alert.Details = newDetails;
                alert.UpdatedOn = Clock.UtcNow;

                var updatedEvent = new LegacyEvents.AlertUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = Clock.UtcNow,
                    RaisedBy = SystemUser.SystemUserId,
                    PersonId = alert.PersonId,
                    Alert = EventModels.Alert.FromModel(alert),
                    OldAlert = new EventModels.Alert
                    {
                        AlertId = alert.AlertId,
                        AlertTypeId = alert.AlertTypeId,
                        Details = oldDetails,
                        ExternalLink = alert.ExternalLink,
                        StartDate = alert.StartDate,
                        EndDate = alert.EndDate
                    },
                    ChangeReason = null,
                    ChangeReasonDetail = null,
                    EvidenceFile = null,
                    Changes = LegacyEvents.AlertUpdatedEventChanges.Details
                };

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
        WithServiceAsync<AlertUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var alertType = (await ReferenceDataCache.GetAlertTypesAsync())
                .SingleRandom(a => !a.InternalOnly);

            var person = await TestData.CreatePersonAsync(p => p
                .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

            var alert = person.Alerts.Single();

            var @event = await WithDbContextAsync(async dbContext =>
            {
                dbContext.Alerts.Attach(alert);

                var oldExternalLink = alert.ExternalLink;
                var newExternalLink = Faker.Internet.Url();
                alert.ExternalLink = newExternalLink;
                alert.UpdatedOn = Clock.UtcNow;

                var updatedEvent = new LegacyEvents.AlertUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = Clock.UtcNow,
                    RaisedBy = SystemUser.SystemUserId,
                    PersonId = alert.PersonId,
                    Alert = EventModels.Alert.FromModel(alert),
                    OldAlert = new EventModels.Alert
                    {
                        AlertId = alert.AlertId,
                        AlertTypeId = alert.AlertTypeId,
                        Details = alert.Details,
                        ExternalLink = oldExternalLink,
                        StartDate = alert.StartDate,
                        EndDate = alert.EndDate
                    },
                    ChangeReason = null,
                    ChangeReasonDetail = null,
                    EvidenceFile = null,
                    Changes = LegacyEvents.AlertUpdatedEventChanges.ExternalLink
                };

                await dbContext.SaveChangesAsync();

                return updatedEvent;
            });

            // Act
            var notification = await mapper.MapEventAsync(@event!);

            // Assert
            Assert.Null(notification);
        });
}
