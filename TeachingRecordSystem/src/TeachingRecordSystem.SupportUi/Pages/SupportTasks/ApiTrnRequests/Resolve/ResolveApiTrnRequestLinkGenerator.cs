namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

public class ResolveApiTrnRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Index", routeValues: new { supportTaskReference });

    public string Matches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Matches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MatchesCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Matches", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string Merge(string supportTaskReference, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Merge", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MergeCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/Merge", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Resolve/CheckAnswers", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);
}
