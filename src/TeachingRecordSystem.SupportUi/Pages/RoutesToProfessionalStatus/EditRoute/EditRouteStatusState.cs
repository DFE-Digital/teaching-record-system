namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public record EditRouteStatusState
{
    public required RouteToProfessionalStatusStatus Status { get; init; }
    public DateOnly? HoldsFrom { get; init; }
    public bool? InductionExemption { get; init; }
    public bool RouteImplicitExemption { get; init; }
}
