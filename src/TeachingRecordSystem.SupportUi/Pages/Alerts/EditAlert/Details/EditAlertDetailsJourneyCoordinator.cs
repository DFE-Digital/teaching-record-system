namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[JourneyCoordinator(JourneyNames.EditAlertDetails, routeValueKeys: ["alertId"])]
public class EditAlertDetailsJourneyCoordinator : JourneyCoordinator<EditAlertDetailsState>
{
    public override EditAlertDetailsState GetStartingState()
    {
        var alertInfo = HttpContext.GetCurrentAlertFeature();

        return new EditAlertDetailsState
        {
            CurrentDetails = alertInfo.Alert.Details,
            Details = alertInfo.Alert.Details
        };
    }
}
