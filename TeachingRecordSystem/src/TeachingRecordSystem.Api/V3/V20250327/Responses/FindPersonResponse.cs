using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250327.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250327.Dtos.QtsInfo;

namespace TeachingRecordSystem.Api.V3.V20250327.Responses;

public record FindPersonResponse
{
    public required int Total { get; init; }
    public required FindPersonRequest Query { get; init; }
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

[AutoMap(typeof(FindPersonsResultItem))]
public record FindPersonResponseResult
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
