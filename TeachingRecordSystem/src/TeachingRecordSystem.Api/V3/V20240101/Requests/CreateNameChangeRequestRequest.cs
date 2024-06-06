using TeachingRecordSystem.Api.V3.Core.Operations;

namespace TeachingRecordSystem.Api.V3.V20240101.Requests;

[AutoMap(typeof(CreateNameChangeRequestCommand), ReverseMap = true)]
public record CreateNameChangeRequestRequest
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
