namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Index", routeValues: new { personId });

    public string Type(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/AddAlert/Type", journeyInstanceId, returnUrl);

    public string Details(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/AddAlert/Details", journeyInstanceId, returnUrl);

    public string Link(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/AddAlert/Link", journeyInstanceId, returnUrl);

    public string StartDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/AddAlert/StartDate", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/AddAlert/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Alerts/AddAlert/CheckAnswers", journeyInstanceId);
}
