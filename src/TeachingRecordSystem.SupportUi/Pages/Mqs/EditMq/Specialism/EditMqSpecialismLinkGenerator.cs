namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public class EditMqSpecialismLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
