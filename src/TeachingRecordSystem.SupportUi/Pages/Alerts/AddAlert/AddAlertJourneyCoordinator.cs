namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[JourneyCoordinator(JourneyNames.AddAlert, routeValueKeys: ["personId"])]
public class AddAlertJourneyCoordinator : JourneyCoordinator<AddAlertState>
{
    public override AddAlertState GetStartingState() => new();
}
