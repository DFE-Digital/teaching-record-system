namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

public class DeleteRouteState
{
    public ChangeReasonOption? ChangeReason { get; set; }

    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();
}
