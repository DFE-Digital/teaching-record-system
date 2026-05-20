namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class AddPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Index");

    public string PersonalDetails(JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/PersonalDetails", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonalDetailsCancel(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/PersonalDetails", "cancel", journeyInstanceId: journeyInstanceId);

    public string Reason(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Reason", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Reason", "cancel", journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/CheckAnswers", "cancel", journeyInstanceId: journeyInstanceId);
}
