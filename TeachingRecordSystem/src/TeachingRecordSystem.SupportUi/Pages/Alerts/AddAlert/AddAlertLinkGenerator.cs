namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Type(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Type", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string TypeCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Type", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Details(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Details", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string DetailsCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Details", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Link(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Link", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string LinkCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Link", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string StartDate(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StartDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/Reason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AddAlert/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
}
