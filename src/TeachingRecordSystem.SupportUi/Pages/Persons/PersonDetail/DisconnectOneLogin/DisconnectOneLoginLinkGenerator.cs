namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

public class DisconnectOneLoginLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId, string oneLoginSubject) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/Index",
            routeValues: new { personId, oneLoginSubject });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/DisconnectOneLogin/Index", journeyInstanceId, returnUrl);

    public string Verified(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/DisconnectOneLogin/Verified", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/DisconnectOneLogin/CheckAnswers", journeyInstanceId);
}
