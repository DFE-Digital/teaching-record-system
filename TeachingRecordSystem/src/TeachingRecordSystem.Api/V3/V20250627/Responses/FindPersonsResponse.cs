using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;
using Alert = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos.Alert;
using NameInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.NameInfo;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.QtsInfo;

namespace TeachingRecordSystem.Api.V3.V20250627.Responses;

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
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
    public required InductionInfo? Induction { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }

    public static FindPersonsResponseResult FromModel(FindPersonsResultItem r) => new()
    {
        Trn = r.Trn,
        DateOfBirth = r.DateOfBirth,
        FirstName = r.FirstName,
        MiddleName = r.MiddleName,
        LastName = r.LastName,
        PreviousNames = r.PreviousNames.Select(n => new NameInfo { FirstName = n.FirstName, MiddleName = n.MiddleName, LastName = n.LastName }).ToArray(),
        Qts = r.Qts.FromModel(),
        Eyts = r.Eyts.FromModel(),
        Alerts = r.Alerts.Select(a => a.FromModel()).ToArray(),
        Induction = r.Induction.FromModel(),
        QtlsStatus = (QtlsStatus)(int)r.QtlsStatus
    };
}
