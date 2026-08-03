namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public class SetStatusLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId, PersonStatus targetStatus) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Index", routeValues: new { personId, targetStatus });

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/SetStatus/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/SetStatus/CheckAnswers", journeyInstanceId);
}
