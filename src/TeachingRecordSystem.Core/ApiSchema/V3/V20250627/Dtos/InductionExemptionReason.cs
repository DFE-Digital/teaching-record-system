using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record InductionExemptionReason
{
    public required Guid InductionExemptionReasonId { get; init; }
    public required string Name { get; init; }

    public static InductionExemptionReason Create(PostgresModels.InductionExemptionReason source) => new()
    {
        InductionExemptionReasonId = source.InductionExemptionReasonId,
        Name = source.Name
    };
}
