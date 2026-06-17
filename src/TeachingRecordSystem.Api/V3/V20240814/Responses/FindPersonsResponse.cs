using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240101;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814.Responses;

public record FindPersonsResponse
{
    public required int Total { get; init; }
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }

    public static FindPersonsResponse Create(FindPersonsResult source) => new()
    {
        Total = source.Total,
        Results = source.Items.Select(i => FindPersonsResponseResult.Create(i)).AsReadOnly()
    };
}

public record FindPersonsResponseResult
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<SanctionInfo> Sanctions { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
    public required DqtInductionStatusInfo InductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }

    public static FindPersonsResponseResult Create(FindPersonsResultItem source) => new()
    {
        Trn = source.Trn,
        DateOfBirth = source.DateOfBirth,
        FirstName = source.FirstName,
        MiddleName = source.MiddleName,
        LastName = source.LastName,
        Sanctions = source.Sanctions.Select(s => SanctionInfo.Create(s)).AsReadOnly(),
        PreviousNames = source.PreviousNames.Select(n => NameInfo.Create(n)).AsReadOnly(),
        InductionStatus = source.DqtInductionStatus is { } dqtInductionStatus
            ? DqtInductionStatusInfo.Create(dqtInductionStatus)
            : null!,
        Qts = source.Qts is { } qts ? QtsInfo.Create(qts) : null,
        Eyts = source.Eyts is { } eyts ? EytsInfo.Create(eyts) : null
    };
}
