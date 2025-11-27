using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class OneLoginUserIdVerificationLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Index");

    public string Details(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage($"/SupportTasks/OneLoginUserIdVerification/Resolve/Index", routeValues: new { supportTaskReference });

    public ResolveOneLoginUserIdVerificationLinkGenerator Resolve { get; } = new(linkGenerator);
}
