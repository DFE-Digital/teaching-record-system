namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionLinkGenerator(LinkGenerator linkGenerator)
{
    public string Status(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StatusCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string ExemptionReasons(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ExemptionReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string StartDate(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StartDateCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CompletedDate(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string CompletedDateCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Reason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckYourAnswers(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CheckYourAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckYourAnswersCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CheckYourAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
}
