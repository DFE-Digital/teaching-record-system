using TeachingRecordSystem.SupportUi.Pages.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string Persons(string? search = null, PersonStatus[]? statuses = null, PersonSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/Index", routeValues: new { search, statuses, sortBy, pageNumber });

    public string PersonCreate() =>
        GetRequiredPathByPage("/Persons/Create/Index");

    public string PersonCreatePersonalDetails(JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/Create/PersonalDetails", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonCreateCreateReason(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/Create/CreateReason", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonCreateCheckAnswers(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/Create/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string PersonCreateCancel(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/Create/PersonalDetails", "cancel", journeyInstanceId: journeyInstanceId);

    public string PersonDetail(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Index", routeValues: new { personId });

    public string PersonEditDetailsPersonalDetails(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/PersonalDetails", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsNameChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/NameChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsOtherDetailsChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/OtherDetailsChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsCheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDetailsCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditDetails/PersonalDetails", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStatus(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStatusCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditExemptionReason(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditExemptionReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/ExemptionReason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStartDate(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditStartDateCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditCompletedDate(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionEditCompletedDateCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/CompletedDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionChangeReason(Guid personId, JourneyInstanceId? journeyInstanceId, JourneyFromCheckYourAnswersPage? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/InductionChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonInductionChangeReasonCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/EditInduction/InductionChangeReason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

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

    public string PersonManualMergeEnterTrn(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/ManualMerge/EnterTrn", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonManualMergeMatches(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/ManualMerge/Matches", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonManualMergeMerge(Guid personId, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/ManualMerge/Merge", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonManualMergeCheckAnswers(Guid personId, JourneyInstanceId? journeyInstanceId = null) =>
        GetRequiredPathByPage("/Persons/ManualMerge/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonManualMergeCancel(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/ManualMerge/EnterTrn", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatus(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/Index", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatusChangeReason(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/ChangeReason", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatusCheckAnswers(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/CheckAnswers", routeValues: new { personId, targetStatus, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonSetStatusCancel(Guid personId, PersonStatus targetStatus, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/SetStatus/ChangeReason", "cancel", routeValues: new { personId, targetStatus }, journeyInstanceId: journeyInstanceId);
}
