using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record EytsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string StatusDescription { get; init; }

    public static async Task<EytsInfo?> Create(dfeta_qtsregistration? qtsRegistration, ReferenceDataCache referenceDataCache)
    {
        if (qtsRegistration is null)
        {
            return null;
        }

        var awardedDate = qtsRegistration.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        if (awardedDate is null)
        {
            return null;
        }

        var earlyYearsStatus = await referenceDataCache.GetEarlyYearsStatusById(qtsRegistration.dfeta_EarlyYearsStatusId.Id);
        var statusDescription = GetStatusDescription(earlyYearsStatus);

        return new()
        {
            Awarded = awardedDate!.Value,
            CertificateUrl = "/v3/certificates/eyts",
            StatusDescription = statusDescription,
        };
    }

    private static string GetStatusDescription(dfeta_earlyyearsstatus earlyYearsStatus) => earlyYearsStatus.dfeta_Value switch
    {
        "222" => "Early years professional status",
        "221" => "Qualified",
        "220" => "Early years trainee",
        _ => throw new ArgumentException($"Unregonized EYTS status: '{earlyYearsStatus.dfeta_Value}'.", nameof(earlyYearsStatus))
    };
}
