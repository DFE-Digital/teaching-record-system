namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        GetPath("/Alerts/AddAlert/Index", new RouteValueDictionary(new { personId }));

    public string Type(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetPath("/Alerts/AddAlert/Type", journeyInstanceId, returnUrl);

    public string Details(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetPath("/Alerts/AddAlert/Details", journeyInstanceId, returnUrl);

    public string Link(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetPath("/Alerts/AddAlert/Link", journeyInstanceId, returnUrl);

    public string StartDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetPath("/Alerts/AddAlert/StartDate", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetPath("/Alerts/AddAlert/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        GetPath("/Alerts/AddAlert/CheckAnswers", journeyInstanceId);

    private string GetPath(string page, JourneyInstanceId journeyInstanceId, string? returnUrl = null)
    {
        var routeValues = new RouteValueDictionary();

        // Add the scoping route values (e.g. personId) before the instance key so that generated
        // URLs read personId first, then returnUrl, then _jid.
        foreach (var kvp in journeyInstanceId.RouteValues.Where(kvp => kvp.Key != JourneyInstanceId.KeyRouteValueName))
        {
            routeValues[kvp.Key] = kvp.Value;
        }

        if (returnUrl is not null)
        {
            routeValues[JourneyCoordinator.ReturnUrlQueryParameterName] = returnUrl;
        }

        routeValues[JourneyInstanceId.KeyRouteValueName] = journeyInstanceId.Key;

        return GetPath(page, routeValues);
    }

    private string GetPath(string page, RouteValueDictionary routeValues) =>
        linkGenerator.GetPathByPage(page, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
