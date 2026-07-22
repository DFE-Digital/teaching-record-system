namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

[JourneyCoordinator(JourneyNames.EditAlertEndDate, routeValueKeys: ["alertId"])]
public class EditAlertEndDateJourneyCoordinator : JourneyCoordinator<EditAlertEndDateState>
{
    public override EditAlertEndDateState GetStartingState()
    {
        var alertInfo = HttpContext.GetCurrentAlertFeature();

        return new EditAlertEndDateState
        {
            CurrentEndDate = alertInfo.Alert.EndDate,
            EndDate = alertInfo.Alert.EndDate
        };
    }
}
