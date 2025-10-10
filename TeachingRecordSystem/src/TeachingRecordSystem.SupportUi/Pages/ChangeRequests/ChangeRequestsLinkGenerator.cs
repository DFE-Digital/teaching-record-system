using TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests;

public class ChangeRequestsLinkGenerator(LinkGenerator linkGenerator)
{
    public EditChangeRequestLinkGenerator EditChangeRequest => new(linkGenerator);
}
