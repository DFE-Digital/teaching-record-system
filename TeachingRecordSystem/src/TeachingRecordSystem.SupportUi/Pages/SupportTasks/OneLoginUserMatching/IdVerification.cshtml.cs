using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching;

public class IdVerification(SupportTaskSearchService searchService, SupportUiLinkGenerator linkGenerator) : PageModel
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
    public OneLoginUserIdVerificationSupportTasksSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }
    public PaginationViewModel? Pagination { get; set; }

    public ResultPage<OneLoginUserIdVerificationSupportTasksSearchResultItem>? Results { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ?? SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ?? OneLoginUserIdVerificationSupportTasksSortByOption.RequestedOn;
        var searchOptions = new OneLoginUserIdVerificationSupportTasksOptions(Search, sortBy, sortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await searchService.SearchOneLoginIdVerificationSupportTasksAsync(
            searchOptions, paginationOptions);

        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification(Search, sortBy, sortDirection, pageNumber));
    }
}
