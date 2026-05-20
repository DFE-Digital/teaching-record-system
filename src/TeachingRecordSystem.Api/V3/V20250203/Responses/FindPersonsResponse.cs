using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;
using NameInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.NameInfo;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;

namespace TeachingRecordSystem.Api.V3.V20250203.Responses;

[AutoMap(typeof(FindPersonsResult))]
public record FindPersonsResponse
{
    public required int Total { get; init; }
    [SourceMember(nameof(FindPersonsResult.Items))]
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
public record FindPersonsResponseResult
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
    [SourceMember("Induction.Status")]
    public required InductionStatus InductionStatus { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }
}
