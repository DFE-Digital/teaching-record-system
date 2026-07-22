namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[JourneyCoordinator(JourneyNames.EditAlertLink, routeValueKeys: ["alertId"])]
public class EditAlertLinkJourneyCoordinator : JourneyCoordinator<EditAlertLinkState>
{
    public override EditAlertLinkState GetStartingState()
    {
        var alertInfo = HttpContext.GetCurrentAlertFeature();

        return new EditAlertLinkState
        {
            CurrentLink = alertInfo.Alert.ExternalLink,
            Link = alertInfo.Alert.ExternalLink
        };
    }
}
