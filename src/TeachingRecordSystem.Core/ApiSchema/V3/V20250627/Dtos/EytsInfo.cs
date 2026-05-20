namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record EytsInfo
{
    public required DateOnly HoldsFrom { get; init; }
    public required IReadOnlyCollection<EytsInfoRoute> Routes { get; init; }
}

public record EytsInfoRoute
{
    public required RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}
