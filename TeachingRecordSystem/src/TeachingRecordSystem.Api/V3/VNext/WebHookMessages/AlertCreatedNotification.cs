using TeachingRecordSystem.Api.V3.V20240920.ApiModels;

namespace TeachingRecordSystem.Api.V3.VNext.WebHookMessages;

public record AlertCreatedNotification
{
    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}
