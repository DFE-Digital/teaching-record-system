using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;

public class IndexModel(SupportTaskSearchService supportTaskSearchService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public ChangeRequestsSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public SupportTaskType[]? ChangeRequestTypes { get; set; }

    public int? TotalRequestCount { get; set; }

    public int? NameChangeRequestCount { get; set; }

    public int? DateOfBirthChangeRequestCount { get; set; }

    public ResultPage<ChangeRequestsSearchResultItem>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    [BindProperty(SupportsGet = true, Name = "_f")]
    public bool FormSubmitted { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var searchOptions = new ChangeRequestsSearchOptions(Search, SortBy, SortDirection, FormSubmitted, ChangeRequestTypes);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await supportTaskSearchService.SearchChangeRequestsAsync(searchOptions, paginationOptions);
        TotalRequestCount = result.TotalRequestCount;
        NameChangeRequestCount = result.NameChangeRequestCount;
        DateOfBirthChangeRequestCount = result.DateOfBirthChangeRequestCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.ChangeRequests.Index(sortBy: SortBy, sortDirection: SortDirection, pageNumber: pageNumber));

        return Page();
    }
}
