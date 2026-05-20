using System.Diagnostics;

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

        // TEMP until we get data fixed up
        var oldestRoute = holdsRoutes.OrderBy(r => r.HoldsFrom).FirstOrDefault();
        Debug.Assert(oldestRoute is null || oldestRoute.HoldsFrom == person.QtsDate);

        return person.EytsDate is not null
            ? new EytsInfo
            {
                HoldsFrom = person.EytsDate.Value,
                CertificateUrl = "/v3/certificates/eyts",
                StatusDescription = "Qualified",
                Routes = holdsRoutes
                    .Select(r => new EytsInfoRoute
                    {
                        RouteToProfessionalStatusType = r.RouteToProfessionalStatusType!
                    })
                    .AsReadOnly()
            }
            : null;
    }
}

public record EytsInfoRoute
{
    public required PostgresModels.RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}

