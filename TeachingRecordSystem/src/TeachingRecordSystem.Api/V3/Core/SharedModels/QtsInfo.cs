using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record QtsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string StatusDescription { get; init; }

    public static async Task<QtsInfo?> CreateAsync(dfeta_qtsregistration? qtsRegistration, ReferenceDataCache referenceDataCache)
    {
        if (qtsRegistration is null)
        {
            return null;
        }

        var awardedDate = qtsRegistration.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        if (awardedDate is null)
        {
            return null;
        }

        var teacherStatus = await referenceDataCache.GetTeacherStatusByIdAsync(qtsRegistration.dfeta_TeacherStatusId.Id);
        var statusDescription = GetStatusDescription(teacherStatus);

        return new()
        {
            Awarded = awardedDate!.Value,
            CertificateUrl = "/v3/certificates/qts",
            StatusDescription = statusDescription,
        };
    }

    private static string GetStatusDescription(dfeta_teacherstatus teacherStatus) => teacherStatus.dfeta_Value switch
    {
        "28" => "Qualified",
        "50" => "Qualified",
        "67" => "Qualified",
        "68" => "Qualified",
        "69" => "Qualified",
        "71" => "Qualified",
        "87" => "Qualified",
        "90" => "Qualified",
        "100" => "Qualified",
        "103" => "Qualified",
        "104" => "Qualified",
        "206" => "Qualified",
        "211" => "Trainee teacher",
        "212" => "Assessment only route candidate",
        "213" => "Qualified",
        "214" => "Partial qualified teacher status",
        "223" => "Qualified",
        _ when teacherStatus.dfeta_name.StartsWith("Qualified teacher", StringComparison.OrdinalIgnoreCase) => "Qualified",
        _ => throw new ArgumentException($"Unregonized QTS status: '{teacherStatus.dfeta_Value}'.", nameof(teacherStatus))
    };
}
