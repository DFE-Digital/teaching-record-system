namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

public class EditAlertStartDateLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Index", routeValues: new { alertId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/EditAlert/StartDate/Index", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/EditAlert/StartDate/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Alerts/EditAlert/StartDate/CheckAnswers", journeyInstanceId);
}
