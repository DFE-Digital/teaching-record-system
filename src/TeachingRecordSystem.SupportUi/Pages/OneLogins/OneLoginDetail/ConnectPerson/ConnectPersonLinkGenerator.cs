namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

public class ConnectPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Index", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string IndexCancel(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Index", handler: "Cancel", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string Match(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Match", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string MatchCancel(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Match", handler: "Cancel", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string Reason(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null, bool fromCheckAnswers = false) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Reason", routeValues: new { oneLoginUserSubject, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/Reason", handler: "Cancel", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/CheckAnswers", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(string oneLoginUserSubject, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/ConnectPerson/CheckAnswers", handler: "Cancel", routeValues: new { oneLoginUserSubject }, journeyInstanceId: journeyInstanceId);
}
