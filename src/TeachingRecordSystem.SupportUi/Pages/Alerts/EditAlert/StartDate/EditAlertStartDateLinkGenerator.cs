namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

public class EditAlertStartDateLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Index", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Index", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Reason", routeValues: new { alertId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/Reason", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/CheckAnswers", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid alertId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/EditAlert/StartDate/CheckAnswers", "cancel", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

}
