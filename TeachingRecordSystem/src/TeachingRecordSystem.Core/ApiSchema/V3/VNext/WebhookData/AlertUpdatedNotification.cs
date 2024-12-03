using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.WebhookData;

public record AlertUpdatedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "alert.updated";

    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}

public class AlertUpdatedNotificationMapper(PersonInfoCache personInfoCache, ReferenceDataCache referenceDataCache) :
    IEventMapper<AlertUpdatedEvent, AlertUpdatedNotification>
{
    public async Task<AlertUpdatedNotification?> MapEventAsync(AlertUpdatedEvent @event)
    {
        if ((@event.Changes & (AlertUpdatedEventChanges.DqtSpent | AlertUpdatedEventChanges.DqtSanctionCode)) != 0)
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

        // We don't expose 'ExternalLink' so if that's the only thing that's changed then don't create a message
        if (@event.Changes == AlertUpdatedEventChanges.ExternalLink)
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
