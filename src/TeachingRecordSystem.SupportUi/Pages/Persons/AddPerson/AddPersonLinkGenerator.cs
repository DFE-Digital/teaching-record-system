namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class AddPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Index");

    public string PersonalDetails(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/AddPerson/PersonalDetails", journeyInstanceId, returnUrl);

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/Persons/AddPerson/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/Persons/AddPerson/CheckAnswers", journeyInstanceId);
}
