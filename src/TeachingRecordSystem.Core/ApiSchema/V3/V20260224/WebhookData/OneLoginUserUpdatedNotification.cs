namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260224.WebhookData;

public record OneLoginUserUpdatedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "one_login_user.updated";

    public required OneLoginUserInfo OneLoginUser { get; init; }
    public required OneLoginUserConnectedPersonInfo? ConnectedPerson { get; init; }
}

public record OneLoginUserInfo
{
    public required string Subject { get; init; }
    public required string? EmailAddress { get; init; }
}

public record OneLoginUserConnectedPersonInfo
{
    public required string Trn { get; init; }
}

public class OneLoginUserUpdatedNotificationMapper(PersonInfoCache personInfoCache) :
    IEventMapper<OneLoginUserUpdatedEvent, OneLoginUserUpdatedNotification>
{
    public async Task<OneLoginUserUpdatedNotification?> MapEventAsync(OneLoginUserUpdatedEvent @event)
    {
        var emailChanged = (@event.Changes & OneLoginUserUpdatedEventChanges.EmailAddress) != 0;
        var personChanged = (@event.Changes & OneLoginUserUpdatedEventChanges.PersonId) != 0;

        if (!emailChanged && !personChanged)
        {
            return null;
        }

        OneLoginUserConnectedPersonInfo? connectedPerson = null;
        if (@event.OneLoginUser.PersonId is Guid personId)
        {
            var person = await personInfoCache.GetRequiredPersonInfoAsync(personId);
            connectedPerson = new OneLoginUserConnectedPersonInfo
            {
                Trn = person.Trn
            };
        }

        return new OneLoginUserUpdatedNotification
        {
            OneLoginUser = new OneLoginUserInfo
            {
                Subject = @event.OneLoginUser.Subject,
                EmailAddress = @event.OneLoginUser.EmailAddress
            },
            ConnectedPerson = connectedPerson
        };
    }
}
