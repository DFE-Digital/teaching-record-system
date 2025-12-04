using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

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
    public ApiTrnRequestsSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }

    public ResultPage<ApiTrnRequestsSearchResultItem>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var searchOptions = new ApiTrnRequestsSearchOptions(Search, SortBy, SortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await supportTaskSearchService.SearchApiTrnRequestsAsync(searchOptions, paginationOptions);
        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.ApiTrnRequests.Index(Search, SortBy, SortDirection, pageNumber));

        return Page();
    }
}
