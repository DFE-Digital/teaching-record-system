namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

[JourneyCoordinator(JourneyNames.CloseAlert, routeValueKeys: ["alertId"])]
public class CloseAlertJourneyCoordinator : JourneyCoordinator<CloseAlertState>
{
    public override CloseAlertState GetStartingState() => new();
}
