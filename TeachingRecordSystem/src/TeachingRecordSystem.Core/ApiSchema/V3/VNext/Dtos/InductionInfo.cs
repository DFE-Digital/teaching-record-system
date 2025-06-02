using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;

namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record InductionInfo
{
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required IReadOnlyCollection<InductionInfoExemption> Exemptions { get; init; }
}

public record InductionInfoExemption
{
    public required InductionExemptionReason? InductionExemptionReason { get; init; }
    public required Guid? QualificationId { get; init; }
}
