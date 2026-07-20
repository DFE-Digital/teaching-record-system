namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionLinkGenerator(LinkGenerator linkGenerator)
{
    public string Status(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, JourneyFromCheckAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StatusCancel(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string ExemptionReasons(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, JourneyFromCheckAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ExemptionReasonsCancel(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string StartDate(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, JourneyFromCheckAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StartDateCancel(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CompletedDate(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, JourneyFromCheckAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string CompletedDateCancel(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Reason(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId, JourneyFromCheckAnswersPage? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Reason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
}
