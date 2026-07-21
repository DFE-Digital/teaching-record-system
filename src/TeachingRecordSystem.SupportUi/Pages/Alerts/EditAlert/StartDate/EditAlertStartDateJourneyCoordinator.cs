namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

[JourneyCoordinator(JourneyNames.EditAlertStartDate, routeValueKeys: ["alertId"])]
public class EditAlertStartDateJourneyCoordinator : JourneyCoordinator<EditAlertStartDateState>
{
    public override EditAlertStartDateState GetStartingState()
    {
        var alertInfo = HttpContext.GetCurrentAlertFeature();

        return new EditAlertStartDateState
        {
            CurrentStartDate = alertInfo.Alert.StartDate,
            StartDate = alertInfo.Alert.StartDate
        };
    }
}
