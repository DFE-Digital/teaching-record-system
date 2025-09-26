using System.ComponentModel;

namespace TeachingRecordSystem.Api.V3.V20240920.Requests;

[Flags]
[Description("Comma-separated list of data to include in response.")]
public enum GetPersonRequestIncludes
{
    None = 0,

    Induction = 1 << 0,
    InitialTeacherTraining = 1 << 1,
    NpqQualifications = 1 << 2,
    MandatoryQualifications = 1 << 3,
    PendingDetailChanges = 1 << 4,
    HigherEducationQualifications = 1 << 5,
    Alerts = 1 << 7,
    PreviousNames = 1 << 8,

    [ExcludeFromSchema]
#pragma warning disable CA1707
    _AllowIdSignInWithProhibitions = 1 << 9,
#pragma warning restore CA1707

    All = Induction | InitialTeacherTraining | NpqQualifications | MandatoryQualifications | PendingDetailChanges | HigherEducationQualifications | Alerts | PreviousNames
}
