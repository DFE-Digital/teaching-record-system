namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

public class ConnectPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Index", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string IndexCancel(string oneLoginUserSubject, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Index", handler: "Cancel", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string Match(string oneLoginUserSubject, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Match", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string MatchCancel(string oneLoginUserSubject, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Match", handler: "Cancel", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);
}
