using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching;

public class RecordMatching(SupportTaskSearchService searchService, SupportUiLinkGenerator linkGenerator) : PageModel
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
    public OneLoginUserRecordMatchingSupportTasksSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }
    public PaginationViewModel? Pagination { get; set; }

    public ResultPage<OneLoginUserRecordMatchingSupportTasksSearchResultItem>? Results { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ?? SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ?? OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn;
        var searchOptions = new OneLoginUserRecordMatchingSupportTasksOptions(Search, sortBy, sortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await searchService.SearchOneLoginUserRecordMatchingSupportTasksAsync(
            searchOptions, paginationOptions);

        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching(Search, sortBy, sortDirection, pageNumber));
    }
}
