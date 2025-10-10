namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public class EditMqProviderLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Provider/Index", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Provider/Index", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Provider/Reason", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Provider/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Provider/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Provider/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
