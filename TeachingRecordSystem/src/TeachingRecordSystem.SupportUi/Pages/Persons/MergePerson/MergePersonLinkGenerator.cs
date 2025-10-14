namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public class MergePersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/Index", routeValues: new { personId });

    public string EnterTrn(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/EnterTrn", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string EnterTrnCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/EnterTrn", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Matches(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/Matches", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MatchesCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/Matches", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Merge(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/Merge", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MergeCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/Merge", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
}
