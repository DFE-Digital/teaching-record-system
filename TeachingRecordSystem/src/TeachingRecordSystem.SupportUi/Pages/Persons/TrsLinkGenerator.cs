using TeachingRecordSystem.SupportUi.Pages.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string Persons(string? search = null, PersonStatus[]? statuses = null, PersonSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/Index", routeValues: new { search, statuses, sortBy, pageNumber });

    public string PersonCreate() =>
        GetRequiredPathByPage("/Persons/AddPerson/Index");

    public string PersonCreatePersonalDetails(JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/AddPerson/PersonalDetails", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonCreateCreateReason(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/AddPerson/Reason", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonCreateCheckAnswers(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/AddPerson/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string PersonCreateCancel(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/AddPerson/PersonalDetails", "cancel", journeyInstanceId: journeyInstanceId);

    public string PersonDetail(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Index", routeValues: new { personId });

    public string PersonEditDetails(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/Index", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsNameChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/NameChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsOtherDetailsChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/OtherDetailsChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsCheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/Index", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStatus(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStatusCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditExemptionReason(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditExemptionReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReasons", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStartDate(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStartDateCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditCompletedDate(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditCompletedDateCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionChangeReason(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Reason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionChangeReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Reason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionCheckYourAnswers(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CheckYourAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionCheckYourAnswersCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CheckYourAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonQualifications(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Qualifications", routeValues: new { personId });

    public string PersonInduction(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Induction", routeValues: new { personId });

    public string PersonAlerts(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Alerts", routeValues: new { personId });

    public string PersonChangeHistory(Guid personId, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/ChangeHistory", routeValues: new { personId, pageNumber });

    public string PersonNotes(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Notes", routeValues: new { personId });

    public string AddNote(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/AddNote", routeValues: new { personId });

    public string PersonMerge(Guid personId) =>
        GetRequiredPathByPage("/Persons/MergePerson/Index", routeValues: new { personId });

    public string PersonMergeEnterTrn(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/MergePerson/EnterTrn", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonMergeMatches(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/MergePerson/Matches", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonMergeMerge(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/MergePerson/Merge", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonMergeCheckAnswers(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/Persons/MergePerson/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonMergeCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/MergePerson/EnterTrn", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatus(Guid personId, PersonStatus targetStatus) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Index", routeValues: new { personId, targetStatus });

    public string PersonSetStatusChangeReason(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Reason", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatusCheckAnswers(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/CheckAnswers", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatusCancel(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Reason", "cancel", routeValues: new { personId, targetStatus }, journeyInstanceId: journeyInstanceId);
}
