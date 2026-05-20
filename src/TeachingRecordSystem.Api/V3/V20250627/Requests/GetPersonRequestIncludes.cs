using System.ComponentModel;

namespace TeachingRecordSystem.Api.V3.V20250627.Requests;

[Flags]
[Description("Comma-separated list of data to include in response.")]
public enum GetPersonRequestIncludes
{
    None = 0,

    Induction = 1 << 0,
    MandatoryQualifications = 1 << 3,
    PendingDetailChanges = 1 << 4,
    Alerts = 1 << 7,
    PreviousNames = 1 << 8,
    [ExcludeFromSchema]
#pragma warning disable CA1707
    _AllowIdSignInWithProhibitions = 1 << 9,
#pragma warning restore CA1707
    RoutesToProfessionalStatuses = 1 << 10,

    All = Induction | RoutesToProfessionalStatuses | MandatoryQualifications | PendingDetailChanges | Alerts | PreviousNames
}
