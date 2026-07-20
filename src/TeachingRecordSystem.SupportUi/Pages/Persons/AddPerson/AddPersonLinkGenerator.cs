namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class AddPersonLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Index");

    public string PersonalDetails(TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/PersonalDetails", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonalDetailsCancel(TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/PersonalDetails", "cancel", journeyInstanceId: journeyInstanceId);

    public string Reason(TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Reason", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/Reason", "cancel", journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/AddPerson/CheckAnswers", "cancel", journeyInstanceId: journeyInstanceId);
}
