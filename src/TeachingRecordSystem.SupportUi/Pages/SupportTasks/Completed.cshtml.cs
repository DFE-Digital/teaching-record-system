using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class Completed(SupportTaskSearchService searchService, SupportTaskService supportTaskService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public SupportTaskType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CompletedByUserId { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public CompletedTasksSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public ResultPage<CompletedTasksSearchResultItem>? Results { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? CompletedByOptions { get; set; }

    public string? OrderedByLabel { get; set; }

    public string? OrderDirectionLabel { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ?? SupportUi.SortDirection.Descending;
        var sortBy = SortBy ?? CompletedTasksSortByOption.CompletedOn;
        var searchOptions = new CompletedTasksSearchOptions(Search, Type, CompletedByUserId, sortBy, sortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await searchService.SearchCompletedTasksAsync(searchOptions, paginationOptions);

        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.Completed(Search, Type, CompletedByUserId, sortBy, sortDirection, pageNumber));

        CompletedByOptions = await supportTaskService.GetAssignableUsersAsync();

        OrderedByLabel = sortBy switch
        {
            CompletedTasksSortByOption.Subject => "subject",
            CompletedTasksSortByOption.CompletedOn => "completed on",
            CompletedTasksSortByOption.TaskType => "type",
            CompletedTasksSortByOption.Outcome => "outcome",
            CompletedTasksSortByOption.CompletedBy => "completed by",
            _ => throw new NotSupportedException($"Unknown sortBy value: '{sortBy}'.")
        };

        OrderDirectionLabel = sortDirection is SupportUi.SortDirection.Ascending ? "ascending" : "descending";
    }
}
