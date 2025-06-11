namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record EytsInfo
{
    public required DateOnly EytsDate { get; init; }
    public required IReadOnlyCollection<EytsInfoRoute> Routes { get; init; }
}

public record EytsInfoRoute
{
    public required RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
}
