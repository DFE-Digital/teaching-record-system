namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record QtsInfo
{
    public required DateOnly AwardedDate { get; init; }
    public required IReadOnlyCollection<QtsInfoAwardedRoute> AwardedRoutes { get; init; }
}

public record QtsInfoAwardedRoute
{
    public required RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
    public required DateOnly AwardedDate { get; init; }
}
