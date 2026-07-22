namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

public class ConnectPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Index", routeValues: new { oneLoginUserSubject });

    public string Index(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/ConnectPerson/Index", journeyInstanceId);

    public string Match(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/ConnectPerson/Match", journeyInstanceId);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/ConnectPerson/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/OneLogins/OneLoginDetail/ConnectPerson/CheckAnswers", journeyInstanceId);
}
