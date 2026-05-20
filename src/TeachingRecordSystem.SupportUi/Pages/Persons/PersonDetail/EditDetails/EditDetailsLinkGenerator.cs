namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public class EditDetailsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/Index", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Cancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/Index", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string NameChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/NameChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NameChangeReasonCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/NameChangeReason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string OtherDetailsChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/OtherDetailsChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string OtherDetailsChangeReasonCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/OtherDetailsChangeReason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

}
