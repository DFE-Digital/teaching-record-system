using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string PersonRoute(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Route", routeValues: new { personId });

    public string DeleteRouteChangeReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/ChangeReason", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string DeleteRouteChangeReasonCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/ChangeReason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string DeleteRouteCheckYourAnswers(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/CheckYourAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string DeleteRouteCheckYourAnswersCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/CheckYourAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string RouteEditDetail(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromInductions = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", routeValues: new { qualificationId, fromInductions }, journeyInstanceId: journeyInstanceId);
    public string RouteEditDetailCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteChangeReason(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/ChangeReason", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteChangeReasonCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/ChangeReason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteCheckYourAnswers(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/CheckYourAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteCheckYourAnswersCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/CheckYourAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteAdd(Guid personId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Index", routeValues: new { personId });
    public string RouteEditStatus(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Status", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditStatusCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Status", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditStartAndEndDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/StartAndEndDates", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditStartAndEndDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/StartAndEndDates", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditHoldsFrom(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/HoldsFrom", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditHoldsFromCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/HoldsFrom", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditDegreeType(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditDegreeTypeCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditTrainingProvider(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/TrainingProvider", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditTrainingProviderCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/TrainingProvider", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditAgeRangeSpecialism(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AgeRangeSpecialism", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditAgeRangeSpecialismCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AgeRangeSpecialism", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditTrainingCountry(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Country", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditTrainingCountryCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Country", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditTrainingSubjects(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditTrainingSubjectsCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditInductionExemption(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/InductionExemption", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditInductionExemptionCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/InductionExemption", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);

    public string RouteAddRoute(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Route", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddRouteCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Route", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddCheckYourAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/CheckYourAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddCheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/CheckYourAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddStatus(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddStatusCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddStartAndEndDate(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/StartAndEndDates", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddStartAndEndDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/StartAndEndDates", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddHoldsFrom(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/HoldsFrom", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddHoldsFromCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/HoldsFrom", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddInductionExemption(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/InductionExemption", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddInductionExemptionCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/InductionExemption", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddTrainingProvider(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/TrainingProvider", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddTrainingProviderCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/TrainingProvider", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddDegreeType(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
    GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/DegreeType", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddDegreeTypeCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/DegreeType", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddCountry(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Country", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddCountryCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
    GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/Country", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddSubjectSpecialisms(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/SubjectSpecialisms", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddSubjectSpecialismsCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/SubjectSpecialisms", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddAgeRangeSpecialism(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/AgeRangeSpecialism", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddAgeRangeSpecialismCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/AgeRangeSpecialism", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
    public string RouteAddChangeReason(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/ChangeReason", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteAddChangeReasonCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/AddRoute/ChangeReason", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string RouteAddPage(AddRoutePage page, Guid personId, JourneyInstanceId journeyInstanceId) =>
        page switch
        {
            AddRoutePage.Route => RouteAddRoute(personId, journeyInstanceId),
            AddRoutePage.Status => RouteAddStatus(personId, journeyInstanceId),
            AddRoutePage.StartAndEndDate => RouteAddStartAndEndDate(personId, journeyInstanceId),
            AddRoutePage.AwardDate => RouteAddHoldsFrom(personId, journeyInstanceId),
            AddRoutePage.InductionExemption => RouteAddInductionExemption(personId, journeyInstanceId),
            AddRoutePage.TrainingProvider => RouteAddTrainingProvider(personId, journeyInstanceId),
            AddRoutePage.DegreeType => RouteAddDegreeType(personId, journeyInstanceId),
            AddRoutePage.Country => RouteAddCountry(personId, journeyInstanceId),
            AddRoutePage.AgeRangeSpecialism => RouteAddAgeRangeSpecialism(personId, journeyInstanceId),
            AddRoutePage.CheckYourAnswers => RouteAddCheckYourAnswers(personId, journeyInstanceId),
            AddRoutePage.SubjectSpecialisms => RouteAddSubjectSpecialisms(personId, journeyInstanceId),
            AddRoutePage.ChangeReason => RouteAddChangeReason(personId, journeyInstanceId),
            _ => throw new ArgumentOutOfRangeException($"{nameof(AddRoutePage)}: {page.ToString()}")
        };
}
