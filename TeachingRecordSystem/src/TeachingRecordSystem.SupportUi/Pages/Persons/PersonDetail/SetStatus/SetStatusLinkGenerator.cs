namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public class SetStatusLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId, PersonStatus targetStatus) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Index", routeValues: new { personId, targetStatus });

    public string Reason(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Reason", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Reason", "cancel", routeValues: new { personId, targetStatus }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/CheckAnswers", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/CheckAnswers", "cancel", routeValues: new { personId, targetStatus }, journeyInstanceId: journeyInstanceId);
}
