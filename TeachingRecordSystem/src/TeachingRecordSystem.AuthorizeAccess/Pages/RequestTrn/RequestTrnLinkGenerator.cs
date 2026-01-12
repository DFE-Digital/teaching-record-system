using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.WebCommon.FormFlow;
using JourneyInstanceId = TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

public abstract class RequestTrnLinkGenerator
{
    public string Index(JourneyInstanceId journeyInstanceId, string? AccessToken = null) =>
        GetRequiredPathByPage("/RequestTrn/Index", routeValues: new { AccessToken }, journeyInstanceId: journeyInstanceId);

    public string TakingNpqRequireTrn(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/TakingNpq", journeyInstanceId: journeyInstanceId);

    public string NpqCheck(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/NpqCheck", journeyInstanceId: journeyInstanceId);

    public string WorkingInSchoolOrEducationalSetting(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/SchoolOrEducationalSetting", journeyInstanceId: journeyInstanceId);

    public string NpqApplication(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NpqApplication", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NpqName(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NpqName", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NotEligible(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/NotEligible", journeyInstanceId: journeyInstanceId);

    public string WorkEmail(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/WorkEmail", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PersonalEmail(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/PersonalEmail", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Name(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/Name", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string PreviousName(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/PreviousName", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string DateOfBirth(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/DateOfBirth", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Identity(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/Identity", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string NationalInsuranceNumber(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NationalInsuranceNumber", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string Address(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/Address", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/CheckAnswers", journeyInstanceId: journeyInstanceId);

    public string NpqTrainingProvider(JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        GetRequiredPathByPage("/RequestTrn/NpqTrainingProvider", routeValues: new { fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string EmailInUse(JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/RequestTrn/EmailInUse", journeyInstanceId: journeyInstanceId);

    public string Submitted(JourneyInstanceId journeyInstanceId) =>
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

public class RoutingRequestTrnLinkGenerator(LinkGenerator linkGenerator) : RequestTrnLinkGenerator
{
    protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) =>
        linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
