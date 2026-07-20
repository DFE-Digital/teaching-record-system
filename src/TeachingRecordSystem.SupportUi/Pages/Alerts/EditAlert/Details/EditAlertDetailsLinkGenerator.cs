namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

public class EditAlertDetailsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/Details/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

}
