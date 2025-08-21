namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string EditChangeRequest(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", routeValues: new { supportTaskReference });

    public string ChangeRequestDocument(string ticketNumber, Guid documentId) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", "documents", routeValues: new { ticketNumber, id = documentId });

    public string AcceptChangeRequest(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Accept", routeValues: new { supportTaskReference });

    public string RejectChangeRequest(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Reject", routeValues: new { supportTaskReference });
}
