using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;

public record AlertUpdatedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "alert.updated";

    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}

public class AlertUpdatedNotificationMapper(PersonInfoCache personInfoCache, ReferenceDataCache referenceDataCache) :
    IEventMapper<Events.AlertUpdatedEvent, AlertUpdatedNotification>
{
    public async Task<AlertUpdatedNotification?> MapEventAsync(Events.AlertUpdatedEvent @event)
    {
        if ((@event.Changes & (Events.AlertUpdatedEventChanges.DqtSpent | Events.AlertUpdatedEventChanges.DqtSanctionCode)) != 0)
        {
            throw new NotSupportedException("Events originating from DQT are not supported.");
        }

        if (@event.Alert.AlertTypeId is not Guid alertTypeId)
        {
            throw new NotSupportedException("Alert does not have an AlertType.");
        }

        var alertType = await referenceDataCache.GetAlertTypeByIdAsync(alertTypeId);
        if (alertType.InternalOnly)
        {
            return null;
        }

        if (@event.Changes == Events.AlertUpdatedEventChanges.ExternalLink)
        {
            return null;
        }

        var person = await personInfoCache.GetRequiredPersonInfoAsync(@event.PersonId);
        var alert = await Alert.FromEventAsync(@event.Alert, referenceDataCache);

        return new()
        {
            Trn = person.Trn,
            Alert = alert
        };
    }
}
