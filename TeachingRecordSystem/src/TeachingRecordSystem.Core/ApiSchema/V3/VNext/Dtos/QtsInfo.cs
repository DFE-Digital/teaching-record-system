namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record QtsInfo
{
    public required DateOnly HoldsFrom { get; init; }
    public required IReadOnlyCollection<QtsInfoRoute> Routes { get; init; }
}

public record QtsInfoRoute
{
    public required RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}
