using System.ComponentModel;
using MediatR;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record GetTeacherRequest : IRequest<GetTeacherResponse?>
{
    public required string Trn { get; init; }
    public required GetTeacherRequestIncludes Include { get; init; } = GetTeacherRequestIncludes.All;
    public required AccessMode AccessMode { get; init; }
}

[Flags]
[Description("Comma-separated list of data to include in response.")]
public enum GetTeacherRequestIncludes
{
    None = 0,

    Induction = 1 << 0,
    InitialTeacherTraining = 1 << 1,
    NpqQualifications = 1 << 2,
    MandatoryQualifications = 1 << 3,
    PendingDetailChanges = 1 << 4,
    HigherEducationQualifications = 1 << 5,
    Sanctions = 1 << 6,
    Alerts = 1 << 7,
    PreviousNames = 1 << 8,

    [ExcludeFromSchema]
    _AllowIdSignInWithProhibitions = 1 << 9,

    All = Induction | InitialTeacherTraining | NpqQualifications | MandatoryQualifications | PendingDetailChanges | HigherEducationQualifications | Sanctions | Alerts | PreviousNames
}
