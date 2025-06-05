using System.ComponentModel;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

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
    _AllowIdSignInWithProhibitions = 1 << 9,
    RoutesToProfessionalStatuses = 1 << 10,

    All = Induction | RoutesToProfessionalStatuses | MandatoryQualifications | PendingDetailChanges | Alerts | PreviousNames
}
