using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;

public record AlertCreatedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "alert.created";

    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}

public class AlertCreatedNotificationMapper(PersonInfoCache personInfoCache, ReferenceDataCache referenceDataCache) :
    IEventMapper<Events.Legacy.AlertCreatedEvent, AlertCreatedNotification>
{
    public async Task<AlertCreatedNotification?> MapEventAsync(Events.Legacy.AlertCreatedEvent @event)
    {
        if (@event.Alert.AlertTypeId is not Guid alertTypeId)
        {
            throw new NotSupportedException("Alert does not have an AlertType.");
        }

        var alertType = await referenceDataCache.GetAlertTypeByIdAsync(alertTypeId);
        if (alertType.InternalOnly)
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
