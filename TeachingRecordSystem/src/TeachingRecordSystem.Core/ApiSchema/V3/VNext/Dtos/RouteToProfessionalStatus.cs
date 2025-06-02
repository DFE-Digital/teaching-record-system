namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record RouteToProfessionalStatus
{
    public required Guid RouteTypeId { get; init; }
    public required string Name { get; init; }
    public required ProfessionalStatusType ProfessionalStatusType { get; init; }
}
