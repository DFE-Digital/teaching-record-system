using TeachingRecordSystem.Api.V3.VNext.ApiModels;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

public record AlertResponse : Alert
{
    public required PersonInfo Person { get; init; }
}
