namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public class EditMqSpecialismLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Specialism/Index", routeValues: new { qualificationId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/Specialism/Index", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/Specialism/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/Specialism/CheckAnswers", journeyInstanceId);
}
