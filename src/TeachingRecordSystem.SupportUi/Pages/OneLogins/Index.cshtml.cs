using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.OneLogins;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins;

[RedactParameters("Search")]
public class IndexModel(OneLoginSearchService searchService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int ResultsPerPage = 20;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Sort by")]
    public OneLoginSearchSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public ResultPage<OneLoginSearchResultItem>? SearchResults { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Search = Search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(Search))
        {
            return Redirect(linkGenerator.Index(selectedTab: "one-logins"));
        }

        var searchOptions = new OneLoginSearchOptions
        {
            Search = Search,
            SortBy = SortBy,
            SortDirection = SortDirection
        };
        var paginationOptions = new PaginationOptions(PageNumber, ResultsPerPage);

        var result = await searchService.SearchAsync(searchOptions, paginationOptions);

        SearchResults = result.Results;

        Pagination = PaginationViewModel.Create(
            SearchResults,
            pageNumber => linkGenerator.OneLogins.Index(Search, SortBy, SortDirection, pageNumber));

        return Page();
    }
}
