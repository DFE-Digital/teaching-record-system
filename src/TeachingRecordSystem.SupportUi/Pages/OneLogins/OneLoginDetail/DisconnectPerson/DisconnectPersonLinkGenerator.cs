namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

public class DisconnectPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string oneLoginUserSubject, Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId,
        bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/Index",
            routeValues: new { oneLoginUserSubject, personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(string oneLoginUserSubject, Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/Index", "cancel",
            routeValues: new { oneLoginUserSubject, personId }, journeyInstanceId: journeyInstanceId);

    public string Verified(string oneLoginUserSubject, Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/Verified",
            routeValues: new { oneLoginUserSubject, personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string VerifiedCancel(string oneLoginUserSubject, Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/Verified", "cancel",
            routeValues: new { oneLoginUserSubject, personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(string oneLoginUserSubject, Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/CheckAnswers",
            routeValues: new { oneLoginUserSubject, personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(string oneLoginUserSubject, Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/OneLoginDetail/DisconnectPerson/CheckAnswers", "cancel",
            routeValues: new { oneLoginUserSubject, personId }, journeyInstanceId: journeyInstanceId);
}
