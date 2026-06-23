using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins;

public class OneLoginsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, OneLoginSearchSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/OneLogins/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public OneLoginDetailLinkGenerator OneLoginDetail { get; } = new(linkGenerator);
}
