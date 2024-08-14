using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Core.Operations;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;
using TeachingRecordSystem.Api.V3.V20240814.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240814.Responses;

[AutoMap(typeof(FindPersonsByTrnAndDateOfBirthResult))]
public record FindPersonsResponse
{
    public required int Total { get; init; }
    [SourceMember(nameof(FindPersonsByTrnAndDateOfBirthResult.Items))]
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsByTrnAndDateOfBirthResultItem))]
public record FindPersonsResponseResult
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<SanctionInfo> Sanctions { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
    public required InductionStatusInfo InductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
}
