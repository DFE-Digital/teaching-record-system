using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;

public class TeacherPensionsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(TeachersPensionsPotentialDuplicatesSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Index", routeValues: new { sortBy, sortDirection, pageNumber });

    public ResolveTeacherPensionsPotentialDuplicateLinkGenerator Resolve { get; } = new(linkGenerator);
}
