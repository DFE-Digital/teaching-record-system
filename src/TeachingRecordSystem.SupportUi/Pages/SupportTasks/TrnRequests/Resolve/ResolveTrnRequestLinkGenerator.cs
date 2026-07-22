namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

public class ResolveTrnRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/Index", routeValues: new { supportTaskReference });

    public string Matches(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TrnRequests/Resolve/Matches", journeyInstanceId, returnUrl);

    public string Merge(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TrnRequests/Resolve/Merge", journeyInstanceId, returnUrl);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TrnRequests/Resolve/CheckAnswers", journeyInstanceId);
}
