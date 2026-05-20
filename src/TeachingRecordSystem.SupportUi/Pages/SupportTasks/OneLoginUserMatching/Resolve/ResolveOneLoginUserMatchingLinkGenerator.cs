namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public class ResolveOneLoginUserMatchingLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Index", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string Verify(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Verify", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string Evidence(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Verify", handler: "Evidence", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ConfirmConnect(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/ConfirmConnect", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ConfirmReject(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/ConfirmReject", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Matches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Matches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NoMatches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/NoMatches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NotConnecting(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/NotConnecting", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ConfirmNotConnecting(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/ConfirmNotConnecting", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Reject(string supportTaskReference, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Reject", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);
}
