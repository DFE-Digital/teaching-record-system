namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class ConnectOneLoginLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string IndexCancel(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Index", handler: "Cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Match(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Match", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MatchCancel(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Match", handler: "Cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Reason", handler: "Cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/CheckAnswers", handler: "Cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
}





