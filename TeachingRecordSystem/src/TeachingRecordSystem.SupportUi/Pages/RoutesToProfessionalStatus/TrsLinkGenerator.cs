namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string PersonRoute(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Route", routeValues: new { personId });
    public string RouteDetail(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteDetailCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
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
    public string RouteEditStartDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/StartDate", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditStartDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/StartDate", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditAwardDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AwardDate", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditAwardDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AwardDate", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditEndDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/EndDate", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditEndDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/EndDate", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditAwardedDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AwardDate", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditAwardedDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AwardDate", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
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
    public string RouteEditStatus(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Status", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditStatusCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Status", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
