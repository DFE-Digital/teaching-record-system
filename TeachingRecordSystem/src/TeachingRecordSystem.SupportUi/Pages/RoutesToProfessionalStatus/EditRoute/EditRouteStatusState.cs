namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class EditRouteStatusState
{
    public ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public bool? InductionExemption { get; set; }
    public bool RouteImplicitExemption { get; init; }
}
