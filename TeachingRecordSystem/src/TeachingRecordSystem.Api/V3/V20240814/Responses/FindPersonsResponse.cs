using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814.Responses;

public record FindPersonsResponse
{
    public required int Total { get; init; }
    public required IReadOnlyCollection<FindPersonsResponseResult> Results { get; init; }

    public static FindPersonsResponse FromModel(FindPersonsResult r) => new()
    {
        Total = r.Total,
        Results = r.Items.Select(FindPersonsResponseResult.FromModel).ToArray()
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

    public static FindPersonsResponseResult FromModel(FindPersonsResultItem r) => new()
    {
        Trn = r.Trn,
        DateOfBirth = r.DateOfBirth,
        FirstName = r.FirstName,
        MiddleName = r.MiddleName,
        LastName = r.LastName,
        Sanctions = r.Sanctions.Select(s => new SanctionInfo { Code = s.Code, StartDate = s.StartDate }).ToArray(),
        PreviousNames = r.PreviousNames.Select(n => new NameInfo { FirstName = n.FirstName, MiddleName = n.MiddleName, LastName = n.LastName }).ToArray(),
        InductionStatus = r.DqtInductionStatus.FromModel()!,
        Qts = r.Qts.FromModel(),
        Eyts = r.Eyts.FromModel()
    };
}
