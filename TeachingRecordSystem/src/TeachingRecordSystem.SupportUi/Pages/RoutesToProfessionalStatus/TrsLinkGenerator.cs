namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string PersonRoute(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Route", routeValues: new { personId });
    public string RouteDetail(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteDetailCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/Detail", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteDegreeType(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteDegreeTypeCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
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
    public string RouteEditEndDate(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/EndDate", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditEndDateCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/EndDate", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteEditDegreeType(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string RouteEditDegreeTypeCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/DegreeType", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
