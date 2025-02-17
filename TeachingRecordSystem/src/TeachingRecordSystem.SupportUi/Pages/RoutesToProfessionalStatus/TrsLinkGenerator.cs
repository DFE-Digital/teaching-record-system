namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string PersonRoute(Guid personId) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Route", routeValues: new { personId });
    public string RouteCheckYourAnswers(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/CheckYourAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string RouteCheckYourAnswersCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/RoutesToProfessionalStatus/EditRoute/CheckYourAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
