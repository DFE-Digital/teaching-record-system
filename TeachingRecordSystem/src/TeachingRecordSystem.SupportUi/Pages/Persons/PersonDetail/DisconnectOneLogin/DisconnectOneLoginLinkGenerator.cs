namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

public class DisconnectOneLoginLinkGenerator(LinkGenerator linkGenerator)
{
    public string DisconnectOneLoginSubject(Guid personId, string oneLoginSubject, JourneyInstanceId? journeyInstanceId,
        bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/Index",
            routeValues: new { personId, oneLoginSubject, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid personId, string oneLoginSubject, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/Index", "cancel",
            routeValues: new { personId, oneLoginSubject }, journeyInstanceId: journeyInstanceId);

    public string Verified(Guid personId, string oneLoginSubject, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/Verified",
            routeValues: new { personId, oneLoginSubject, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string VerifiedCancel(Guid personId, string oneLoginSubject, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/Verified", "cancel",
            routeValues: new { personId, oneLoginSubject }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, string oneLoginSubject, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/CheckAnswers",
            routeValues: new { personId, oneLoginSubject }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, string oneLoginSubject, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/CheckAnswers", "cancel",
            routeValues: new { personId, oneLoginSubject }, journeyInstanceId: journeyInstanceId);
}
