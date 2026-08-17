using Microsoft.Extensions.Options;
using CoreTrnRequestStatus = TeachingRecordSystem.Core.Models.TrnRequestStatus;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260224.WebhookData;

public record TrnRequestCompletedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "trn_request.completed";

    public required TrnRequestCompletedNotificationTrnRequestInfo TrnRequest { get; init; }
}

// These are named after the notification rather than the thing they describe so that they don't collide with the
// same-named endpoint DTOs when both are generated into a version's OpenAPI document.
public record TrnRequestCompletedNotificationTrnRequestInfo
{
    public required string RequestId { get; init; }
    public required string? Trn { get; init; }
    public required TrnRequestCompletedNotificationTrnRequestStatus Status { get; init; }
    public required bool PotentialDuplicate { get; init; }
    public required string? AccessYourTeachingQualificationsLink { get; init; }
}

public enum TrnRequestCompletedNotificationTrnRequestStatus
{
    Pending = 0,
    Completed = 1,
    Rejected = 2,
    Dormant = 3
}

public class TrnRequestCompletedNotificationMapper(
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptions,
    PersonInfoCache personInfoCache) :
    IEventMapper<TrnRequestUpdatedEvent, TrnRequestCompletedNotification>
{
    public async Task<TrnRequestCompletedNotification?> MapEventAsync(TrnRequestUpdatedEvent @event)
    {
        var statusChanged = (@event.Changes & TrnRequestUpdatedChanges.Status) != 0;
        var newStatusIsCompleted = @event.TrnRequest.Status == CoreTrnRequestStatus.Completed;

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
            TrnRequest = new TrnRequestCompletedNotificationTrnRequestInfo
            {
                RequestId = @event.RequestId,
                Trn = trn,
                Status = TrnRequestCompletedNotificationTrnRequestStatus.Completed,
                PotentialDuplicate = @event.TrnRequest.PotentialDuplicate ?? false,
                AccessYourTeachingQualificationsLink = aytqLink
            }
        };
    }
}
