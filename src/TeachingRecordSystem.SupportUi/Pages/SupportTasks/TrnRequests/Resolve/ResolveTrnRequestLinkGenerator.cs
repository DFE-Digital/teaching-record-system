namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

public class ResolveTrnRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/Index", routeValues: new { supportTaskReference });

    public string Matches(string supportTaskReference, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/Matches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MatchesCancel(string supportTaskReference, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/Matches", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string Merge(string supportTaskReference, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/Merge", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MergeCancel(string supportTaskReference, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/Merge", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(string supportTaskReference, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(string supportTaskReference, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Resolve/CheckAnswers", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);
}
