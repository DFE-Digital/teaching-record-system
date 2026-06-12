namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260612.WebhookData;

public record PersonDeactivatedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "person.deactivated";

    public required PersonDeactivatedNotificationPersonInfo DeactivatedPerson { get; init; }
    public required PersonDeactivatedNotificationPersonInfo? MergedWithPerson { get; init; }
}

public record PersonDeactivatedNotificationPersonInfo
{
    public required string Trn { get; init; }
}

public class PersonDeactivatedNotificationMapper(PersonInfoCache personInfoCache) :
    IEventMapper<PersonDeactivatedEvent, PersonDeactivatedNotification>
{
    public async Task<PersonDeactivatedNotification?> MapEventAsync(PersonDeactivatedEvent @event)
    {
        var deactivatedPerson = await personInfoCache.GetRequiredPersonInfoAsync(@event.PersonId);
        var mergedWithPerson = @event.MergedWithPersonId is Guid mergedWithPersonId ? await personInfoCache.GetRequiredPersonInfoAsync(mergedWithPersonId) : null;

        return new PersonDeactivatedNotification
        {
            DeactivatedPerson = new() { Trn = deactivatedPerson.Trn },
            MergedWithPerson = mergedWithPerson is not null ? new PersonDeactivatedNotificationPersonInfo { Trn = mergedWithPerson.Trn } : null
        };
    }
}
