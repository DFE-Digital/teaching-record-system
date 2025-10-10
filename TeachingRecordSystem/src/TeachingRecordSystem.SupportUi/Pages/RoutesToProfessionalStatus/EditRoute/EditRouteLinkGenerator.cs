namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class EditRouteLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId, bool? fromInductions = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Index", routeValues: new { qualificationId, fromInductions });
    public string Detail(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromInductions = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", routeValues: new { qualificationId, fromInductions }, journeyInstanceId: journeyInstanceId);
    public string DetailCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string Route(Guid qualificationId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Route", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Route", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string Status(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Status", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string StatusCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Status", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string StartAndEndDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/StartAndEndDates", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string StartAndEndDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/StartAndEndDates", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string HoldsFrom(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/HoldsFrom", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string HoldsFromCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/HoldsFrom", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string DegreeType(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string DegreeTypeCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string Country(Guid qualificationId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Country", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string CountryCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Country", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string SubjectSpecialisms(Guid qualificationId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string SubjectSpecialismsCancel(Guid qualificationId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string TrainingProvider(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/TrainingProvider", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string TrainingProviderCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/TrainingProvider", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string AgeRangeSpecialism(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AgeRangeSpecialism", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string AgeRangeSpecialismCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/AgeRangeSpecialism", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string TrainingCountry(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Country", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string TrainingCountryCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Country", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string TrainingSubjects(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string TrainingSubjectsCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/SubjectSpecialisms", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string InductionExemption(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/InductionExemption", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string InductionExemptionCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/InductionExemption", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string Reason(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Reason", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string ReasonCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string CheckYourAnswers(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/CheckYourAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string CheckYourAnswersCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/CheckYourAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
