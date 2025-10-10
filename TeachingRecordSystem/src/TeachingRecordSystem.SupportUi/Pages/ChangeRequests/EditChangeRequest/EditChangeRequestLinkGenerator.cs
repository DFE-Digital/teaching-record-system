namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

public class EditChangeRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", routeValues: new { supportTaskReference });

    public string Evidence(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Index", "evidence", routeValues: new { supportTaskReference });

    public string Accept(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Accept", routeValues: new { supportTaskReference });

    public string Reject(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/ChangeRequests/EditChangeRequest/Reject", routeValues: new { supportTaskReference });
}
