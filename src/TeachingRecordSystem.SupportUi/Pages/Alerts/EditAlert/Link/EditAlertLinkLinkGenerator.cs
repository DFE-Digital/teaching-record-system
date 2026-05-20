namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

public class EditAlertLinkLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Link/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Link/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid alertId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Link/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Link/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Link/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Link/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

}
