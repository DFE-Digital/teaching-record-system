namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

public class ReopenAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/ReopenAlert/Index", routeValues: new { alertId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/ReopenAlert/Index", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Alerts/ReopenAlert/CheckAnswers", journeyInstanceId);
}
