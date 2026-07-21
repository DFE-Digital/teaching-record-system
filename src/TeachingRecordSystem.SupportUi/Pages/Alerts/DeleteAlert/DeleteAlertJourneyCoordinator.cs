namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

[JourneyCoordinator(JourneyNames.DeleteAlert, routeValueKeys: ["alertId"])]
public class DeleteAlertJourneyCoordinator : JourneyCoordinator<DeleteAlertState>
{
    public override DeleteAlertState GetStartingState() => new();
}
