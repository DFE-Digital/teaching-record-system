using System.Diagnostics;

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
            .Where(r => r.RouteToProfessionalStatusType!.ProfessionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && r.Status is RouteToProfessionalStatusStatus.Holds)
            .ToArray();

        // TEMP until we get data fixed up
        var oldestRoute = holdsRoutes.OrderBy(r => r.HoldsFrom).FirstOrDefault();
        Debug.Assert(oldestRoute is null || oldestRoute.HoldsFrom == person.QtsDate);

        return new QtsInfo()
        {
            HoldsFrom = person.QtsDate!.Value,
            CertificateUrl = "/v3/certificates/qts",
            StatusDescription = oldestRoute?.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.QtlsAndSetMembershipId
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
}

public record QtsInfoRoute
{
    public required PostgresModels.RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}
