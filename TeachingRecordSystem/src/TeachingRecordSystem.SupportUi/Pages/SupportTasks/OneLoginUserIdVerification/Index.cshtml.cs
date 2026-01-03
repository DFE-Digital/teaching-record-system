using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class Index(SupportTaskSearchService searchService, SupportUiLinkGenerator linkGenerator) : PageModel
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
    public OneLoginIdVerificationSupportTasksSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }
    public PaginationViewModel? Pagination { get; set; }

    public ResultPage<OneLoginIdVerificationSupportTasksSearchResultItem>? Results { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await searchService.SearchOneLoginIdVerificationSupportTasksAsync(
            new SearchOneLoginUserIdVerificationSupportTasksOptions(
                SortBy ??= OneLoginIdVerificationSupportTasksSortByOption.SupportTaskReference,
                sortDirection),
            paginationOptions);

        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.OneLoginUserIdVerification.Index(SortBy, SortDirection, pageNumber));
    }
}
