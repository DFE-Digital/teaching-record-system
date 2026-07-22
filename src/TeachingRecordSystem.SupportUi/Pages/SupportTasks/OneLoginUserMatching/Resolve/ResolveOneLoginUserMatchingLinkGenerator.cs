namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public class ResolveOneLoginUserMatchingLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Index", routeValues: new { supportTaskReference });

    public string Verify(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/Verify", journeyInstanceId, returnUrl);

    public string Evidence(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/Resolve/Evidence", routeValues: new { supportTaskReference });

    public string ConfirmConnect(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/ConfirmConnect", journeyInstanceId);

    public string ConfirmReject(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/ConfirmReject", journeyInstanceId);

    public string Matches(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/Matches", journeyInstanceId);

    public string NoMatches(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/NoMatches", journeyInstanceId);

    public string NotConnecting(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/NotConnecting", journeyInstanceId, returnUrl);

    public string ConfirmNotConnecting(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/ConfirmNotConnecting", journeyInstanceId);

    public string Reject(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/OneLoginUserMatching/Resolve/Reject", journeyInstanceId, returnUrl);
}
