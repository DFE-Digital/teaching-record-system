namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record EytsInfo
{
    public required DateOnly AwardedDate { get; init; }
    public required IReadOnlyCollection<EytsInfoAwardedRoute> AwardedRoutes { get; init; }
}

public record EytsInfoAwardedRoute
{
    public required RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
    public required DateOnly AwardedDate { get; init; }
}
