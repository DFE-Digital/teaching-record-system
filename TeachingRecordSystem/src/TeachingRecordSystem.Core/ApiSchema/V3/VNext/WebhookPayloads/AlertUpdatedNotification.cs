using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.WebhookPayloads;

public record AlertUpdatedNotification
{
    public required string Trn { get; init; }
    public required Alert Alert { get; init; }
}
