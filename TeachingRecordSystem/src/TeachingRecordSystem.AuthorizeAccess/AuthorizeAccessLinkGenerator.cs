using JourneyInstanceId = GovUk.Questions.AspNetCore.JourneyInstanceId;

namespace TeachingRecordSystem.AuthorizeAccess;

public abstract class AuthorizeAccessLinkGenerator
{
    public string DebugIdentity(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/DebugIdentity", journeyInstanceId: journeyInstanceId);

    public string NotVerified(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/NotVerified", journeyInstanceId: journeyInstanceId);

    public string Connect(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Connect", journeyInstanceId: journeyInstanceId);

    public string NationalInsuranceNumber(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetRequiredPathByPage("/NationalInsuranceNumber", routeValues: new { returnUrl }, journeyInstanceId: journeyInstanceId);

    public string Trn(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetRequiredPathByPage("/Trn", routeValues: new { returnUrl }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string SupportRequestSubmitted(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SupportRequestSubmitted", journeyInstanceId: journeyInstanceId);

    public string Found(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Found", journeyInstanceId: journeyInstanceId);

    public string NotFound(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/NotFound", journeyInstanceId: journeyInstanceId);

    public string SignOut(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/SignOut", journeyInstanceId: journeyInstanceId);

    public string Name(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetRequiredPathByPage("/Name", routeValues: new { returnUrl }, journeyInstanceId: journeyInstanceId);

    public string DateOfBirth(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetRequiredPathByPage("/DateOfBirth", routeValues: new { returnUrl }, journeyInstanceId: journeyInstanceId);

    public string NoTrn(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/NoTrn", journeyInstanceId: journeyInstanceId);

    public string PendingSupportRequest(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/PendingSupportRequest", journeyInstanceId: journeyInstanceId);

    public string ProofOfIdentity(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        GetRequiredPathByPage("/ProofOfIdentity", routeValues: new { returnUrl }, journeyInstanceId: journeyInstanceId);

    protected virtual string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var combinedRouteValues = new RouteValueDictionary(routeValues);

        if (journeyInstanceId is not null)
        {
            foreach (var kvp in journeyInstanceId.RouteValues)
            {
                combinedRouteValues[kvp.Key] = kvp.Value;
            }
        }

        return GetRequiredPathByPage(page, handler, combinedRouteValues);
    }

    protected abstract string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null);
}

public class RoutingAuthorizeAccessLinkGenerator(LinkGenerator linkGenerator) : AuthorizeAccessLinkGenerator
{
    protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) =>
        linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
