namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public class ResolveOneLoginUserIdVerificationLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/Index", routeValues: new { supportTaskReference });

    public string ConfirmConnect(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/ConfirmConnect", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);

    public string ConfirmReject(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/ConfirmReject", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);

    public string Matches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/Matches", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);

    public string NoMatches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/NoMatches", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);

    public string NotConnecting(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/NotConnecting", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);

    public string NotConnectingReason(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/NotConnectingReason", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);

    public string Reject(string supportTaskReference, JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Resolve/Reject", routeValues: new { supportTaskReference, returnUrl }, journeyInstanceId: journeyInstanceId);
}
