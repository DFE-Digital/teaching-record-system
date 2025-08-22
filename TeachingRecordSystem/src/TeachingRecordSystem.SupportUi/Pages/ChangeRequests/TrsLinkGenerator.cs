namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string EditChangeRequest(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", routeValues: new { supportTaskReference });

    public string ChangeRequestEvidence(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", "evidence", routeValues: new { supportTaskReference });

    public string AcceptChangeRequest(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Accept", routeValues: new { supportTaskReference });

    public string RejectChangeRequest(string supportTaskReference) => GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Reject", routeValues: new { supportTaskReference });
}
