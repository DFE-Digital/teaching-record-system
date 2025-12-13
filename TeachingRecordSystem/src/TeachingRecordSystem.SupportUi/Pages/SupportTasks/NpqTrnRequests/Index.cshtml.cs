using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class Index(SupportTaskSearchService supportTaskSearchService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public NpqTrnRequestsSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }

    public ResultPage<NpqTrnRequestsSearchResultItem>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var searchOptions = new NpqTrnRequestsSearchOptions(Search, SortBy, SortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await supportTaskSearchService.SearchNpqTrnRequestsAsync(searchOptions, paginationOptions);
        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(Results,
            pageNumber => linkGenerator.SupportTasks.NpqTrnRequests.Index(Search, SortBy, SortDirection, pageNumber));

        return Page();
    }
}
