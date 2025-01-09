namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class RouteToProfessionalStatus
{
    public required Guid RouteToProfessionalStatusId { get; init; }
    public required string Name { get; init; }
    public required QualificationType QualificationType { get; init; }
    public required bool IsActive { get; set; }
}
