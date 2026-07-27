namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public class EditMqStartDateLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/StartDate/Index", routeValues: new { qualificationId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/StartDate/Index", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/StartDate/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/StartDate/CheckAnswers", journeyInstanceId);
}
