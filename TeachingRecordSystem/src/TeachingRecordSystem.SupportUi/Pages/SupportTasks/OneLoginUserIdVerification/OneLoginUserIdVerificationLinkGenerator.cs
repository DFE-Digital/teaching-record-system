namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class OneLoginUserIdVerificationLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Index");

    public string ResolveDetails(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/Index", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ResolveNoMatches(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/NoMatches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ResolveMatches(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/Matches", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ResolveMatchesCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/Matches", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);
}
