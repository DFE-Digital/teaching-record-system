namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public class EditDetailsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/Index", routeValues: new { personId });

    public string Index(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditDetails/Index", journeyInstanceId, returnUrl);

    public string NameChangeReason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditDetails/NameChangeReason", journeyInstanceId, returnUrl);

    public string OtherDetailsChangeReason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditDetails/OtherDetailsChangeReason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/PersonDetail/EditDetails/CheckAnswers", journeyInstanceId);
}
