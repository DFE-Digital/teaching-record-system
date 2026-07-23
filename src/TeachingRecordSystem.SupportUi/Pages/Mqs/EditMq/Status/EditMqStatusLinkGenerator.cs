namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public class EditMqStatusLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/EditMq/Status/Index", routeValues: new { qualificationId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/Status/Index", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/Status/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Mqs/EditMq/Status/CheckAnswers", journeyInstanceId);
}
