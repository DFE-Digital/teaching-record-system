namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class ConnectOneLoginLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Index", routeValues: new { personId });

    public string Index(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/ConnectOneLogin/Index", journeyInstanceId);

    public string Match(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/ConnectOneLogin/Match", journeyInstanceId);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/ConnectOneLogin/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/ConnectOneLogin/CheckAnswers", journeyInstanceId);
}
