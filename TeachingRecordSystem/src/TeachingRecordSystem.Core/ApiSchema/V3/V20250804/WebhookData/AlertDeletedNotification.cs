using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;

public record AlertDeletedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "alert.deleted";

    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}

public class AlertDeletedNotificationMapper(PersonInfoCache personInfoCache, ReferenceDataCache referenceDataCache) :
    IEventMapper<AlertDeletedEvent, AlertDeletedNotification>
{
    public async Task<AlertDeletedNotification?> MapEventAsync(AlertDeletedEvent @event)
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

        var person = await personInfoCache.GetPersonInfoAsync(@event.PersonId);
        var alert = await Alert.FromEventAsync(@event.Alert, referenceDataCache);

        return new()
        {
            Trn = person.Trn,
            Alert = alert
        };
    }
}
