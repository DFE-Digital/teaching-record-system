namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public record DeleteRouteToProfessionalStatusOptions
{
    public required Guid QualificationId { get; init; }
}
