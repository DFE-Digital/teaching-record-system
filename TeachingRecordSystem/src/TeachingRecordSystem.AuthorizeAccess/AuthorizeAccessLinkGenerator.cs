using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess;

public abstract class AuthorizeAccessLinkGenerator
{
    public string DebugIdentity(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/DebugIdentity", journeyInstanceId: journeyInstanceId);

    public string NotVerified(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/NotVerified", journeyInstanceId: journeyInstanceId);

    public string Connect(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Connect", journeyInstanceId: journeyInstanceId);

    public string NationalInsuranceNumber(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/NationalInsuranceNumber", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Trn(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/Trn", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

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

    public string RequestTrn(JourneyInstanceId journeyInstanceId, string? AccessToken = null) =>
        GetRequiredPathByPage("/RequestTrn/Index", routeValues: new { AccessToken }, journeyInstanceId: journeyInstanceId);

    public string TakingNpqRequireTrn(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/TakingNpq", journeyInstanceId: journeyInstanceId);

    public string RequestTrnNpqCheck(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/NpqCheck", journeyInstanceId: journeyInstanceId);

    public string RequestTrnWorkingInSchoolOrEducationalSetting(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/SchoolOrEducationalSetting", journeyInstanceId: journeyInstanceId);

    public string RequestTrnNpqApplication(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NpqApplication", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnNpqName(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NpqName", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnNotEligible(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/NotEligible", journeyInstanceId: journeyInstanceId);

    public string RequestTrnWorkEmail(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/WorkEmail", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnPersonalEmail(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/PersonalEmail", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnName(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/Name", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnPreviousName(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/PreviousName", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnDateOfBirth(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/DateOfBirth", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnIdentity(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/Identity", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnNationalInsuranceNumber(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NationalInsuranceNumber", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnAddress(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/Address", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnCheckAnswers(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string RequestTrnNpqTrainingProvider(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NpqTrainingProvider", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string RequestTrnEmailInUse(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/EmailInUse", journeyInstanceId: journeyInstanceId);

    public string RequestTrnSubmitted(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/Submitted", journeyInstanceId: journeyInstanceId);

    protected virtual string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var url = GetRequiredPathByPage(page, handler, routeValues);

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }

    protected abstract string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null);
}

public class RoutingAuthorizeAccessLinkGenerator(LinkGenerator linkGenerator) : AuthorizeAccessLinkGenerator
{
    protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) =>
        linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
