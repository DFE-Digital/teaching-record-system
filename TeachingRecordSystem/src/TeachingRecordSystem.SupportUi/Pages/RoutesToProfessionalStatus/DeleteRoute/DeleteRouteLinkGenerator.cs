namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

public class DeleteRouteLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/Index", routeValues: new { qualificationId });
    public string Reason(Guid qualificationId, JourneyInstanceId? journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/Reason", routeValues: new { qualificationId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
    public string ReasonCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/Reason", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string CheckAnswers(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/CheckAnswers", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
    public string CheckAnswersCancel(Guid qualificationId, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/CheckAnswers", "cancel", routeValues: new { qualificationId }, journeyInstanceId: journeyInstanceId);
}
