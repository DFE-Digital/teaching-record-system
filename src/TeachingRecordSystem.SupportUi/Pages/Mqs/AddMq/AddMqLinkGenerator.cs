namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class AddMqLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Index", routeValues: new { personId });

    public string Provider(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/AddMq/Provider", journeyInstanceId, returnUrl);

    public string Specialism(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/AddMq/Specialism", journeyInstanceId, returnUrl);

    public string StartDate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/AddMq/StartDate", journeyInstanceId, returnUrl);

    public string Status(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/AddMq/Status", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Mqs/AddMq/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Mqs/AddMq/CheckAnswers", journeyInstanceId);
}
