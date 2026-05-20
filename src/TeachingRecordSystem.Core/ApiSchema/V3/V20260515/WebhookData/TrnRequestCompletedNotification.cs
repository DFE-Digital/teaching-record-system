using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos;
using CoreTrnRequestStatus = TeachingRecordSystem.Core.Models.TrnRequestStatus;
using TrnRequestStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos.TrnRequestStatus;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260515.WebhookData;

public record TrnRequestCompletedNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "trn_request.completed";

    public required TrnRequestInfo TrnRequest { get; init; }
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
            TrnRequest = new TrnRequestInfo
            {
                RequestId = @event.RequestId,
                OneLoginUserSubject = @event.TrnRequest.OneLoginUserSubject,
                Trn = trn,
                Status = TrnRequestStatus.Completed,
                PotentialDuplicate = @event.TrnRequest.PotentialDuplicate ?? false,
                AccessYourTeachingQualificationsLink = aytqLink
            }
        };
    }
}
