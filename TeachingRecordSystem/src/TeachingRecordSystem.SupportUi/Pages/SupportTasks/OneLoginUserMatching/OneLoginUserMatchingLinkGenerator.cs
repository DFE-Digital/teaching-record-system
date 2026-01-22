using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching;

public class OneLoginUserMatchingLinkGenerator(LinkGenerator linkGenerator)
{
    public string IdVerification() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/IdVerification");

    public string IdVerification(string? search = null, OneLoginIdVerificationSupportTasksSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/IdVerification", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public string RecordMatching() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/OneLoginUserMatching/RecordMatching");

    public ResolveOneLoginUserMatchingLinkGenerator Resolve { get; } = new(linkGenerator);
}
