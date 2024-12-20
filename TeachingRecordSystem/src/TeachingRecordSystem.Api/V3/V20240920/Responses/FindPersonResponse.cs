using AutoMapper.Configuration.Annotations;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240920.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using Alert = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos.Alert;

namespace TeachingRecordSystem.Api.V3.V20240920.Responses;

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
    [SourceMember(nameof(FindPersonsResultItem.DqtInductionStatus))]
    public required DqtInductionStatusInfo InductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
}
