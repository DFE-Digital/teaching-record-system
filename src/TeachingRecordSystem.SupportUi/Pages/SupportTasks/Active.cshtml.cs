using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class Active(
    SupportTaskSearchService searchService,
    SupportTaskService supportTaskService,
    IOptions<SupportTaskAssignmentOptions> assignmentOptions,
    SupportUiLinkGenerator linkGenerator) :
    PageModel
{
    private const int TasksPerPage = 20;

    // The name shared by the checkbox for every task on this page and by the hidden inputs that
    // carry the tasks selected on other pages.
    private const string SelectedTaskInputName = "SupportTaskReference";

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

    // The tasks the user has selected, on this page and on any others they've paged through.
    [BindProperty(SupportsGet = true, Name = SelectedTaskInputName)]
    public string[] SelectedTaskReferences { get; set; } = [];

    public int? TotalTaskCount { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public ResultPage<SupportTasksSearchResultItem>? Results { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

    public bool ShowMyselfOption { get; set; }

    public Guid UnassignedUserId => SupportTaskSearchService.UnassignedUserId;

    public Guid CurrentUserId => User.GetUserId();

    public string? OrderedByLabel { get; set; }

    public string? OrderDirectionLabel { get; set; }

    public string SelectedTaskInputsSelector => $"[name={SelectedTaskInputName}]";

    // Selected tasks that aren't shown on this page. These are rendered as hidden inputs so that the
    // selection survives a change of page and is submitted along with the checkboxes for the tasks
    // that are shown. Tasks on this page are deliberately excluded - their checkbox is the only
    // record of whether they're selected, so unticking one deselects it.
    public IReadOnlyCollection<string> SelectedTaskReferencesNotOnPage { get; set; } = [];

    public string? ReturnUrl { get; set; }

    // Past the first page the back link steps back through the results rather than leaving the list,
    // keeping the selection with it. On the first page there's nowhere left to step back to.
    public string? BackLinkUrl { get; set; }

    public bool BackLinkUsesHtmx { get; set; }

    // The back link sits outside main, so an ordinary htmx swap has to update it out of band. Not on
    // a history restore though: that replaces the whole body, and htmx lifts out of band elements out
    // of the response before swapping it in, which would leave the restored page without one.
    public bool SwapBackLinkOutOfBand => Request.Headers["HX-History-Restore-Request"] != "true";

    // Where the 'clear selection' link in the selection banner goes: this page, same filters, no
    // selection. The banner is rendered both with the page and on its own by OnGetSelectionBanner.
    public string ClearSelectionUrl => linkGenerator.SupportTasks.Active(Type, AssignedToUserId, Status, SortBy, SortDirection, PageNumber);

    public string SelectionBannerUrl => linkGenerator.SupportTasks.ActiveSelectionBanner(Type, AssignedToUserId, Status, SortBy, SortDirection, PageNumber);

    private HashSet<string> SelectedTaskReferenceLookup { get; set; } = [];

    public int SelectedTaskCount => SelectedTaskReferenceLookup.Count;

    public bool IsSelected(string supportTaskReference) => SelectedTaskReferenceLookup.Contains(supportTaskReference);

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

        SelectedTaskReferences = SelectedTaskReferences.Distinct().ToArray();
        SelectedTaskReferenceLookup = SelectedTaskReferences.ToHashSet();

        var referencesOnPage = Results.Select(r => r.SupportTaskReference).ToHashSet();
        SelectedTaskReferencesNotOnPage = SelectedTaskReferences.Where(r => !referencesOnPage.Contains(r)).AsReadOnly();

        ReturnUrl = GetReturnUrl();

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.Active(Type, AssignedToUserId, Status, sortBy, sortDirection, pageNumber));

        BackLinkUsesHtmx = Results.CurrentPage > 1;

        BackLinkUrl = BackLinkUsesHtmx
            ? linkGenerator.SupportTasks.Active(Type, AssignedToUserId, Status, sortBy, sortDirection, Results.CurrentPage - 1)
            : linkGenerator.Index();

        var assignableUsers = await supportTaskService.GetAssignableUsersAsync(
            includeAdministrators: assignmentOptions.Value.IncludeAdministrators,
            includeCurrentAssignees: true);

        ShowMyselfOption = assignableUsers.Any(u => u.UserId == CurrentUserId);

        AssignToOptions = assignableUsers
            .Where(u => u.UserId != CurrentUserId)
            .AsReadOnly();

        OrderedByLabel = sortBy switch
        {
            SupportTasksSortByOption.SupportTaskReference => "task reference",
            SupportTasksSortByOption.Subject => "subject",
            SupportTasksSortByOption.TaskType => "type",
            SupportTasksSortByOption.Status => "status",
            SupportTasksSortByOption.AssignedTo => "assigned to",
            SupportTasksSortByOption.RequestedOn => "requested on",
            SupportTasksSortByOption.Source => "source",
            _ => throw new NotSupportedException($"Unknown sortBy value: '{sortBy}'.")
        };

        OrderDirectionLabel = sortDirection is SupportUi.SortDirection.Ascending ? "ascending" : "descending";
    }

    public IActionResult OnGetSelectionBanner([FromQuery(Name = SelectedTaskInputName)] string[] supportTaskReferences)
    {
        var selectedTaskReferences = supportTaskReferences.Distinct().ToArray();

        // Keep the address bar in step with the selection. Ticking a checkbox doesn't otherwise touch
        // the URL, and the URL is what the page gets rebuilt from when the user navigates back to it.
        Response.Htmx(headers => headers.ReplaceUrl(
            QueryHelpers.AddQueryString(
            ClearSelectionUrl,
            new Dictionary<string, StringValues> { { SelectedTaskInputName, selectedTaskReferences } })));

        return Partial("_SelectedActiveSupportTasks", new SelectionViewModel(selectedTaskReferences.Length, ClearSelectionUrl));
    }

    // Links away from this page come back to it via a return URL. The selected task references are
    // in the query string when the user has paged through the results; they'd be repeated in every
    // link on the page (and can get long enough to break them), so leave them out.
    private string GetReturnUrl()
    {
        var query = QueryHelpers.ParseQuery(Request.QueryString.Value);
        query.Remove(SelectedTaskInputName);
        return QueryHelpers.AddQueryString(Request.Path, query);
    }

    public record SelectionViewModel(int SelectedTaskCount, string ClearSelectionUrl);
}
