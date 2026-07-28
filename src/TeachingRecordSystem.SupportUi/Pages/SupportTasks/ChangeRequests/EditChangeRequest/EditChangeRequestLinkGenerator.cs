namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

public class EditChangeRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ChangeRequests/EditChangeRequest/Index", routeValues: new { supportTaskReference, returnUrl });

    public string Evidence(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ChangeRequests/EditChangeRequest/Index", "evidence", routeValues: new { supportTaskReference });

    public string Accept(string supportTaskReference, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ChangeRequests/EditChangeRequest/Accept", routeValues: new { supportTaskReference, returnUrl });

    public string Reject(string supportTaskReference, string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ChangeRequests/EditChangeRequest/Reject", routeValues: new { supportTaskReference, returnUrl });
}
