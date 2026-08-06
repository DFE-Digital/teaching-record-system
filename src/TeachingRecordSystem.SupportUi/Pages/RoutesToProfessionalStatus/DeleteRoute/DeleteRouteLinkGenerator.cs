namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

public class DeleteRouteLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid qualificationId) =>
        linkGenerator.GetRequiredPathByPage("/RoutesToProfessionalStatus/DeleteRoute/Index", routeValues: new { qualificationId });

    public string Reason(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/DeleteRoute/Reason", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/RoutesToProfessionalStatus/DeleteRoute/CheckAnswers", journeyInstanceId, returnUrl);
}
