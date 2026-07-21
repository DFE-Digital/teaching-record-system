namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

public class EditAlertDetailsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/Index", routeValues: new { alertId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/EditAlert/Details/Index", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Alerts/EditAlert/Details/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Alerts/EditAlert/Details/CheckAnswers", journeyInstanceId);
}
