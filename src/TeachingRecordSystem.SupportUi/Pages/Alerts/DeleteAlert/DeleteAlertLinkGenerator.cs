namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/DeleteAlert/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/DeleteAlert/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/DeleteAlert/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/DeleteAlert/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);
}
