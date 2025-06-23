using System.Diagnostics;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record QtsInfo
{
    public required DateOnly HoldsFrom { get; init; }
    public required string CertificateUrl { get; init; }
    public required string StatusDescription { get; init; }
    public required int AwardedOrApprovedCount { get; init; }
    public required IReadOnlyCollection<QtsInfoRoute> Routes { get; init; }

    public static QtsInfo? Create(PostgresModels.Person person)
    {
        if (person.QtsDate is null)
        {
            return null;
        }

        var routes = person.Qualifications?.OfType<PostgresModels.RouteToProfessionalStatus>()
            ?? throw new InvalidOperationException("Qualifications not loaded.");

        var holdsRoutes = routes
            .Where(r => r.Status is RouteToProfessionalStatusStatus.Holds)
            .ToArray();

        var oldestRoute = holdsRoutes.OrderBy(r => r.HoldsFrom).First();
        Debug.Assert(oldestRoute.HoldsFrom == person.QtsDate);

        return new QtsInfo()
        {
            HoldsFrom = oldestRoute.HoldsFrom.Value,
            CertificateUrl = "/v3/certificates/qts",
            StatusDescription = oldestRoute.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.QtlsAndSetMembershipId
                ? "Qualified Teacher Learning and Skills status"
                : "Qualified",
            AwardedOrApprovedCount = holdsRoutes.Length,
            Routes = holdsRoutes
                .Select(r => new QtsInfoRoute()
                {
                    RouteToProfessionalStatusType = r.RouteToProfessionalStatusType!
                })
                .AsReadOnly()
        };
    }

    public static async Task<QtsInfo?> CreateAsync(dfeta_qtsregistration[] qtsRegistrations, DateTime? qtlsDate, ReferenceDataCache referenceDataCache)
    {
        var awardedQts = await qtsRegistrations
            .Where(q => q.dfeta_QTSDate is not null)
            .ToAsyncEnumerable()
            .SelectAwait(async q =>
            {
                var teacherStatus = await referenceDataCache.GetTeacherStatusByIdAsync(q.dfeta_TeacherStatusId.Id);
                return (Date: q.dfeta_QTSDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true), Description: GetStatusDescriptionForTeacherStatus(teacherStatus));
            })
            .ToListAsync();

        if (qtlsDate is not null)
        {
            awardedQts.Add((qtlsDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true), "Qualified Teacher Learning and Skills status"));
        }

        if (awardedQts.Count == 0)
        {
            return null;
        }

        var effectiveQts = awardedQts.OrderBy(q => q.Date).First();

        return new()
        {
            HoldsFrom = effectiveQts.Date,
            CertificateUrl = "/v3/certificates/qts",
            StatusDescription = effectiveQts.Description,
            AwardedOrApprovedCount = awardedQts.Count,
            Routes = []
        };
    }

    private static string GetStatusDescriptionForTeacherStatus(dfeta_teacherstatus teacherStatus) =>
        teacherStatus.dfeta_Value switch
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

public record QtsInfoRoute
{
    public required PostgresModels.RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}
