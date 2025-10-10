using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

public class ApiTrnRequestsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() => linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Index");

    public ResolveApiTrnRequestLinkGenerator Resolve { get; } = new(linkGenerator);
}
