using System.Diagnostics;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record EytsInfo
{
    public required DateOnly HoldsFrom { get; init; }
    public required string CertificateUrl { get; init; }
    public required string StatusDescription { get; init; }
    public required IReadOnlyCollection<EytsInfoRoute> Routes { get; init; }

    public static EytsInfo? Create(PostgresModels.Person person)
    {
        if (person.EytsDate is null)
        {
            return null;
        }

        var routes = person.Qualifications?.OfType<PostgresModels.RouteToProfessionalStatus>()
            ?? throw new InvalidOperationException("Qualifications not loaded.");

        var holdsRoutes = routes
            .Where(r => r.RouteToProfessionalStatusType!.ProfessionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && r.Status is RouteToProfessionalStatusStatus.Holds)
            .ToArray();

        var oldestRoute = holdsRoutes.OrderBy(r => r.HoldsFrom).First();
        Debug.Assert(oldestRoute.HoldsFrom == person.EytsDate);

        return person.EytsDate is not null
            ? new EytsInfo()
            {
                HoldsFrom = person.EytsDate.Value,
                CertificateUrl = "/v3/certificates/eyts",
                StatusDescription = "Qualified",
                Routes = holdsRoutes
                    .Select(r => new EytsInfoRoute()
                    {
                        RouteToProfessionalStatusType = r.RouteToProfessionalStatusType!
                    })
                    .AsReadOnly()
            }
            : null;
    }

    public static async Task<EytsInfo?> CreateAsync(IEnumerable<dfeta_qtsregistration> qtsRegistrations, ReferenceDataCache referenceDataCache)
    {
        var qtsRegistration = qtsRegistrations.OrderByDescending(x => x.CreatedOn).FirstOrDefault(qts => qts.dfeta_EYTSDate is not null);

        if (qtsRegistration is null)
        {
            return null;
        }

        var awardedDate = qtsRegistration.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        if (awardedDate is null)
        {
            return null;
        }

        var earlyYearsStatus = await referenceDataCache.GetEarlyYearsStatusByIdAsync(qtsRegistration.dfeta_EarlyYearsStatusId.Id);
        var statusDescription = GetStatusDescription(earlyYearsStatus);

        return new()
        {
            HoldsFrom = awardedDate.Value,
            CertificateUrl = "/v3/certificates/eyts",
            StatusDescription = statusDescription,
            Routes = []
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

public record EytsInfoRoute
{
    public required PostgresModels.RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}

