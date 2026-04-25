using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250203.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;

namespace TeachingRecordSystem.Api.V3.V20250203.Responses;

public record FindPersonResponse
{
    public required int Total { get; init; }
    public required FindPersonRequest Query { get; init; }
    public required IReadOnlyCollection<FindPersonResponseResult> Results { get; init; }
}

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
    public required InductionStatus InductionStatus { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }

    public static FindPersonResponseResult FromModel(FindPersonsResultItem r) => new()
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
        InductionStatus = (InductionStatus)(int)r.Induction.Status,
        QtlsStatus = (QtlsStatus)(int)r.QtlsStatus
    };
}
