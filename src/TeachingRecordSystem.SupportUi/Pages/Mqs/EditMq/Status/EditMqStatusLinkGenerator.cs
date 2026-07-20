namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public class EditMqStatusLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid qualificationId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid qualificationId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid qualificationId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid qualificationId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid qualificationId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
