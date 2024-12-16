using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record QtsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string StatusDescription { get; init; }

    public static async Task<QtsInfo?> CreateAsync(dfeta_qtsregistration? qtsRegistration, ReferenceDataCache referenceDataCache, DateTime? qtlsDate)
    {
        var qtsDates = new List<DateTime?>() { qtsRegistration?.dfeta_QTSDate, qtlsDate };
        var earliestDate = qtsDates.Where(x => x != null).OrderBy(date => date).FirstOrDefault();
        if (earliestDate is null && qtsRegistration is null)
        {
            return null;
        }

        var teacherStatus = qtsRegistration != null ? await referenceDataCache.GetTeacherStatusByIdAsync(qtsRegistration.dfeta_TeacherStatusId.Id) : null;
        var statusDescription = GetStatusDescription(teacherStatus, qtsRegistration?.dfeta_QTSDate, qtlsDate);

        return new()
        {
            Awarded = earliestDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            CertificateUrl = "/v3/certificates/qts",
            StatusDescription = statusDescription,
        };
    }

    private static string GetStatusDescription(dfeta_teacherstatus? teacherStatus, DateTime? qtsDate = null, DateTime? qtlsDate = null)
    {
        return (qtsDate, qtlsDate) switch
        {
            (null, null) => GetStatusDescriptionForTeacherStatus(teacherStatus),
            (var qts, var qtls) when qtls.ToDateOnlyWithDqtBstFix(isLocalTime: true) <= qts.ToDateOnlyWithDqtBstFix(isLocalTime: true) => "Qualified Teacher Learning and Skills status",
            (null, var qtls) => "Qualified Teacher Learning and Skills status",
            _ => GetStatusDescriptionForTeacherStatus(teacherStatus),
        };

        string GetStatusDescriptionForTeacherStatus(dfeta_teacherstatus? teacherStatus) =>
            teacherStatus!.dfeta_Value switch
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
                _ => throw new ArgumentException($"Unrecognized QTS status: '{teacherStatus.dfeta_Value}'.", nameof(teacherStatus))
            };
    }

}
