using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using PersonInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos.PersonInfo;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

public record AlertResponse : Alert
{
    public required PersonInfo Person { get; init; }
}
