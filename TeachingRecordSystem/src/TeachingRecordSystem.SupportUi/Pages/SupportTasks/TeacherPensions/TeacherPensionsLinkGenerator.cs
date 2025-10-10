using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;

public class TeacherPensionsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(TeacherPensionsSortOptions? sortBy = null, SortDirection? sortDirection = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Index", routeValues: new { sortBy, sortDirection });

    public ResolveTeacherPensionsPotentialDuplicateLinkGenerator Resolve { get; } = new(linkGenerator);
}
