using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Services.OneLogins;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins;

[RedactParameters("Search")]
public class IndexModel(OneLoginSearchService searchService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Sort by")]
    public OneLoginSearchSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public SortDirection? SortDirection { get; set; }

    public IReadOnlyList<OneLoginSearchResultItem>? SearchResults { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Search = Search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(Search))
        {
            return Redirect(linkGenerator.Index(selectedTab: "one-logins"));
        }

        var result = await searchService.SearchAsync(new OneLoginSearchOptions
        {
            Search = Search,
            SortBy = SortBy,
            SortDirection = SortDirection
        });

        SearchResults = result.Results;

        return Page();
    }
}
