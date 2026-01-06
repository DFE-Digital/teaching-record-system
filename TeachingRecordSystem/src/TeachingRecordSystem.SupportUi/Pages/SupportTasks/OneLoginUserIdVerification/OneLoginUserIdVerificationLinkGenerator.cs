using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class OneLoginUserIdVerificationLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Index");

    public string Index(string? search = null, OneLoginIdVerificationSupportTasksSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserIdVerification/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public ResolveOneLoginUserIdVerificationLinkGenerator Resolve { get; } = new(linkGenerator);
}
