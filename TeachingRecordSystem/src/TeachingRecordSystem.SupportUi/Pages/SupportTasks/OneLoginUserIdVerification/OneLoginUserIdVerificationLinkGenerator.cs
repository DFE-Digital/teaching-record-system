using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class OneLoginUserIdVerificationLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Index");

    public string Index(OneLoginIdVerificationRequestsSortByOption sortBy, SortDirection sortDirection) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Index", routeValues: new { sortBy, sortDirection });

    public ResolveOneLoginUserIdVerificationLinkGenerator Resolve { get; } = new(linkGenerator);
}
