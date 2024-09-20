using TeachingRecordSystem.Api.V3.V20240920.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

public record AlertResponse : Alert
{
    public required PersonInfo Person { get; init; }
}
