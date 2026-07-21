namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/DeleteAlert/Index", routeValues: new { alertId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/DeleteAlert/Index", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Alerts/DeleteAlert/CheckAnswers", journeyInstanceId);
}
