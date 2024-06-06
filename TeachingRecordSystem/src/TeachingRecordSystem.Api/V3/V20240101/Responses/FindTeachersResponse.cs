using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;
using TeachingRecordSystem.Api.V3.V20240101.Requests;

namespace TeachingRecordSystem.Api.V3.V20240101.Responses;

[AutoMap(typeof(FindTeachersResult))]
public record FindTeachersResponse
{
    public required int Total { get; init; }
    public required FindTeachersRequest Query { get; init; }
    public required IReadOnlyCollection<FindTeachersResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindTeachersResultItem))]
public record FindTeachersResponseResult
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<SanctionInfo> Sanctions { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
}
