namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public class DeleteMqLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/DeleteMq/Index", routeValues: new { qualificationId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/DeleteMq/Index", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Mqs/DeleteMq/CheckAnswers", journeyInstanceId);
}
