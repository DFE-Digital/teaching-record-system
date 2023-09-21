using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi;

public class TrsLinkGenerator
{
    private readonly LinkGenerator _linkGenerator;

    public TrsLinkGenerator(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public string Index() => GetRequiredPathByPage("/Index");

    public string SignOut() => GetRequiredPathByPage("/SignOut");

    public string SignedOut() => GetRequiredPathByPage("/SignedOut");

    public string Alert(Guid alertId) => GetRequiredPathByPage("/Alerts/Alert/Index", routeValues: new { alertId });

    public string CloseAlert(Guid alertId) => GetRequiredPathByPage("/Alerts/Alert/Close", routeValues: new { alertId });

    public string Cases() => GetRequiredPathByPage("/Cases/Index");

    public string EditCase(string ticketNumber) => GetRequiredPathByPage("/Cases/EditCase/Index", routeValues: new { ticketNumber });

    public string CaseDocument(string ticketNumber, Guid documentId) => GetRequiredPathByPage("/Cases/EditCase/Index", "documents", routeValues: new { ticketNumber, id = documentId });

    public string AcceptCase(string ticketNumber) => GetRequiredPathByPage("/Cases/EditCase/Accept", routeValues: new { ticketNumber });

    public string RejectCase(string ticketNumber) => GetRequiredPathByPage("/Cases/EditCase/Reject", routeValues: new { ticketNumber });

    public string Persons(string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/Index", routeValues: new { search, sortBy, pageNumber });

    public string PersonDetail(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Index", routeValues: new { personId, search, sortBy, pageNumber });

    public string PersonAlerts(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/Alerts", routeValues: new { personId, search, sortBy, pageNumber });

    public string PersonAddAlert(Guid personId) => GetRequiredPathByPage("/Persons/PersonDetail/AddAlert", routeValues: new { personId });

    public string PersonChangeLog(Guid personId, string? search = null, ContactSearchSortByOption? sortBy = null, int? pageNumber = null) =>
        GetRequiredPathByPage("/Persons/PersonDetail/ChangeLog", routeValues: new { personId, search, sortBy, pageNumber });

    public string Users() => GetRequiredPathByPage("/Users/Index");

    public string AddUser() => GetRequiredPathByPage("/Users/AddUser/Index");

    public string AddUser(string userId) => GetRequiredPathByPage("/Users/AddUser/Confirm", routeValues: new { userId });

    public string EditUser(Guid userId) => GetRequiredPathByPage("/Users/EditUser", routeValues: new { userId });

    private string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) =>
        _linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
