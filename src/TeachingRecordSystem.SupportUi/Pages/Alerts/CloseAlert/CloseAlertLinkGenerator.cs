namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public class CloseAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/CloseAlert/Index", routeValues: new { alertId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/CloseAlert/Index", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/CloseAlert/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Alerts/CloseAlert/CheckAnswers", journeyInstanceId);
}
