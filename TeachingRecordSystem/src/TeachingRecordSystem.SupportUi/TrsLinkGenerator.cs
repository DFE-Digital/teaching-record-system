using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;

namespace TeachingRecordSystem.SupportUi;

public class TrsLinkGenerator
{
    protected const string DateOnlyFormat = DateOnlyModelBinder.Format;

    private readonly LinkGenerator _linkGenerator;

    public TrsLinkGenerator(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public string Index() => GetRequiredPathByPage("/Index");

    public string SignOut() => GetRequiredPathByPage("/SignOut");

    public string SignedOut() => GetRequiredPathByPage("/SignedOut");

    public string Alert(Guid alertId) => GetRequiredPathByPage("/Alerts/Alert/Index", routeValues: new { alertId });

    public string AlertAdd(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertAddConfirm(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/AddAlert/Confirm", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string AlertClose(Guid alertId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/Index", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string AlertCloseConfirm(Guid alertId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Alerts/CloseAlert/Confirm", routeValues: new { alertId }, journeyInstanceId: journeyInstanceId);

    public string Cases() => GetRequiredPathByPage("/Cases/Index");

    public string EditCase(string ticketNumber) => GetRequiredPathByPage("/Cases/EditCase/Index", routeValues: new { ticketNumber });

    public string CaseDocument(string ticketNumber, Guid documentId) => GetRequiredPathByPage("/Cases/EditCase/Index", "documents", routeValues: new { ticketNumber, id = documentId });

    public string AcceptCase(string ticketNumber) => GetRequiredPathByPage("/Cases/EditCase/Accept", routeValues: new { ticketNumber });

    public string RejectCase(string ticketNumber) => GetRequiredPathByPage("/Cases/EditCase/Reject", routeValues: new { ticketNumber });

    public string MqAdd(Guid personId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Index", routeValues: new { personId });

    public string MqAddProvider(Guid personId, JourneyInstanceId? journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Provider", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddProviderCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Provider", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddSpecialism(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Specialism", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddSpecialismCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Specialism", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddStartDate(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/StartDate", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddStartDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddResult(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Result", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddResultCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/Result", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddCheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/CheckAnswers", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string MqAddCheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        GetRequiredPathByPage("/Mqs/AddMq/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Persons(string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/Index", routeValues: new { search, sortBy, pageNumber });

    public string PersonDetail(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Index", routeValues: new { personId, search, sortBy, pageNumber });

    public string PersonQualifications(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Qualifications", routeValues: new { personId, search, sortBy, pageNumber });

    public string PersonAlerts(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Alerts", routeValues: new { personId, search, sortBy, pageNumber });

    public string PersonChangeLog(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/ChangeLog", routeValues: new { personId, search, sortBy, pageNumber });

    public string PersonEditName(Guid personId, JourneyInstanceId? journeyInstanceId) => GetRequiredPathByPage("/Persons/PersonDetail/EditName/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonEditNameConfirm(Guid personId, JourneyInstanceId journeyInstanceId) => GetRequiredPathByPage("/Persons/PersonDetail/EditName/Confirm", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDateOfBirth(Guid personId, JourneyInstanceId? journeyInstanceId) => GetRequiredPathByPage("/Persons/PersonDetail/EditDateOfBirth/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string PersonEditDateOfBirthConfirm(Guid personId, JourneyInstanceId journeyInstanceId) => GetRequiredPathByPage("/Persons/PersonDetail/EditDateOfBirth/Confirm", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Users() => GetRequiredPathByPage("/Users/Index");

    public string AddUser() => GetRequiredPathByPage("/Users/AddUser/Index");

    public string AddUser(string userId) => GetRequiredPathByPage("/Users/AddUser/Confirm", routeValues: new { userId });

    public string EditUser(Guid userId) => GetRequiredPathByPage("/Users/EditUser", routeValues: new { userId });

    private string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var url = _linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, FormFlow.Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }
}
