namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

[JourneyCoordinator(JourneyNames.ReopenAlert, routeValueKeys: ["alertId"])]
public class ReopenAlertJourneyCoordinator : JourneyCoordinator<ReopenAlertState>
{
    public override ReopenAlertState GetStartingState() => new();
}
