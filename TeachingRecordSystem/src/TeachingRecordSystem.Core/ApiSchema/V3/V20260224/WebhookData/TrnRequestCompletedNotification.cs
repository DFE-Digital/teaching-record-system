using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260224.WebhookData;

public record TrnRequestCompletedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "trn_request.completed";

    public required TrnRequestInfo TrnRequest { get; init; }
}

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required string? Trn { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public required bool PotentialDuplicate { get; init; }
    public required string? AccessYourTeachingQualificationsLink { get; init; }
}

public class TrnRequestCompletedNotificationMapper(
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptions,
    PersonInfoCache personInfoCache) :
    IEventMapper<TrnRequestUpdatedEvent, TrnRequestCompletedNotification>
{
    public async Task<TrnRequestCompletedNotification?> MapEventAsync(TrnRequestUpdatedEvent @event)
    {
        var statusChanged = (@event.Changes & TrnRequestUpdatedChanges.Status) != 0;
        var newStatusIsCompleted = @event.TrnRequest.Status == TrnRequestStatus.Completed;

        if (!statusChanged || !newStatusIsCompleted)
        {
            return null;
        }

        string? trn = null;
        if (@event.TrnRequest.ResolvedPersonId is Guid personId)
        {
            var person = await personInfoCache.GetRequiredPersonInfoAsync(personId);
            trn = person.Trn;
        }

        var trnToken = @event.TrnRequest.TrnToken;
        var aytqLink = trnToken is not null
            ? $"{aytqOptions.Value.BaseAddress}{aytqOptions.Value.StartUrlPath}?trn_token={Uri.EscapeDataString(trnToken)}"
            : null;

        return new TrnRequestCompletedNotification
        {
            TrnRequest = new TrnRequestInfo
            {
                RequestId = @event.RequestId,
                Trn = trn,
                Status = TrnRequestStatus.Completed,
                PotentialDuplicate = @event.TrnRequest.PotentialDuplicate ?? false,
                AccessYourTeachingQualificationsLink = aytqLink
            }
        };
    }
}
