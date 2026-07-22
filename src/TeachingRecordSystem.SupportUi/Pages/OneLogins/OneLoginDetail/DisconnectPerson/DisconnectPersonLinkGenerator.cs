namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

public class DisconnectPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject, Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/Index",
            routeValues: new { oneLoginUserSubject, personId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/DisconnectPerson/Index", journeyInstanceId, returnUrl);

    public string Verified(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/DisconnectPerson/Verified", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/DisconnectPerson/CheckAnswers", journeyInstanceId);
}
