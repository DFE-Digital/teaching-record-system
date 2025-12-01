namespace TeachingRecordSystem.Core.Services.InductionExemptions;

public class RouteWithExemption
{
    public required Guid RouteToProfessionalStatusId { get; init; }
    public required Guid InductionExemptionReasonId { get; init; }
    public required string RouteToProfessionalStatusName { get; init; }
    public required string InductionExemptionReasonName { get; init; }
}
