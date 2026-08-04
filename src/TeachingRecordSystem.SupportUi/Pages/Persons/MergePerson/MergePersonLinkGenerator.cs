namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public class MergePersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/MergePerson/Index", routeValues: new { personId });

    public string EnterTrn(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/MergePerson/EnterTrn", journeyInstanceId, returnUrl);

    public string Matches(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/MergePerson/Matches", journeyInstanceId, returnUrl);

    public string Merge(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/MergePerson/Merge", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/MergePerson/CheckAnswers", journeyInstanceId);
}
