using TeachingRecordSystem.Api.V3.VNext.ApiModels;

namespace TeachingRecordSystem.Api.V3.VNext.WebHookMessages;

public record AlertDeletedNotification
{
    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}
