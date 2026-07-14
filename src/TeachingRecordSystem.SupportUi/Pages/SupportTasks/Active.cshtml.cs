using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class Active(SupportTaskSearchService searchService, SupportTaskService supportTaskService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    public SupportTaskType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public SupportTaskStatus[]? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? AssignedToUserId { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SupportTasksSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public ResultPage<SupportTasksSearchResultItem>? Results { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ?? SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ?? SupportTasksSortByOption.RequestedOn;
        var statuses = Status?.Length > 0 ? Status : [SupportTaskStatus.Open, SupportTaskStatus.InProgress];
        var searchOptions = new SupportTasksSearchOptions(Type, AssignedToUserId, statuses, sortBy, sortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await searchService.SearchSupportTasksAsync(searchOptions, paginationOptions);

        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.Active(Type, AssignedToUserId, Status, sortBy, sortDirection, pageNumber));

        AssignToOptions = await supportTaskService.GetAssignableUsersAsync();
    }
}
