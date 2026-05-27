namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.WebhookData;

public record PersonDeactivatedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "person.deactivated";

    public required TrnRequestCompletedNotificationPersonInfo DeactivatedPerson { get; init; }
    public required TrnRequestCompletedNotificationPersonInfo? MergedWithPerson { get; init; }
}

public record TrnRequestCompletedNotificationPersonInfo
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
            MergedWithPerson = mergedWithPerson is not null ? new TrnRequestCompletedNotificationPersonInfo { Trn = mergedWithPerson.Trn } : null
        };
    }
}
