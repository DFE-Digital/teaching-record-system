using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record RouteToProfessionalStatusType
{
    public required Guid RouteToProfessionalStatusTypeId { get; init; }
    public required string Name { get; init; }
    public required ProfessionalStatusType ProfessionalStatusType { get; init; }

    public static RouteToProfessionalStatusType Create(PostgresModels.RouteToProfessionalStatusType source) => new()
    {
        RouteToProfessionalStatusTypeId = source.RouteToProfessionalStatusTypeId,
        Name = source.Name,
        ProfessionalStatusType = ProfessionalStatusType.Create(source.ProfessionalStatusType)
    };
}
