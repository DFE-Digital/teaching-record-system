using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240101;
using TeachingRecordSystem.Api.V3.V20250627.Requests;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;
using InductionInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.InductionInfo;
using NameInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.NameInfo;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.QtsInfo;

namespace TeachingRecordSystem.Api.V3.V20250627.Responses;

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
    public required InductionInfo? Induction { get; init; }
    public required QtlsStatus QtlsStatus { get; set; }

    public static FindPersonResponseResult Create(FindPersonsResultItem source) => new()
    {
        Trn = source.Trn,
        DateOfBirth = source.DateOfBirth,
        FirstName = source.FirstName,
        MiddleName = source.MiddleName,
        LastName = source.LastName,
        PreviousNames = source.PreviousNames.Select(n => NameInfo.Create(n)).AsReadOnly(),
        Qts = source.Qts is { } qts ? QtsInfo.Create(qts) : null,
        Eyts = source.Eyts is { } eyts ? EytsInfo.Create(eyts) : null,
        Alerts = source.Alerts.Select(a => Alert.Create(a)).AsReadOnly(),
        Induction = InductionInfo.Create(source.Induction),
        QtlsStatus = QtlsStatus.Create(source.QtlsStatus)
    };
}
