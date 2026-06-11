using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests;

public class Index(SupportTaskSearchService supportTaskSearchService, TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public Guid[] ApplicationUserIds { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public TrnRequestsSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int? TotalTaskCount { get; set; }

    public ResultPage<TrnRequestsSearchResultItem>? Results { get; set; }

    public IReadOnlyCollection<SourceApplicationFacetValue>? ResultsBySourceApplication { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public bool ShowFilters { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var searchOptions = new TrnRequestsSearchOptions(
            Search,
            ApplicationUserIds,
            SortBy,
            SortDirection);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await supportTaskSearchService.SearchTrnRequestsAsync(searchOptions, paginationOptions);
        TotalTaskCount = result.TotalTaskCount;
        Results = result.SearchResults;

        // Ensure we cover all application users that generate TRN requests, even if there are no active requests for them right now
        var allTrnRequestingSourceApplications = await dbContext.TrnRequestMetadata
            .Select(t => new { t.ApplicationUser!.UserId, t.ApplicationUser.Name, t.ApplicationUser.ShortName })
            .Distinct()
            .ToArrayAsync();

        ResultsBySourceApplication = allTrnRequestingSourceApplications
            .LeftJoin(
                result.BySourceApplication,
                a => a.UserId,
                r => r.ApplicationUserId,
                (user, results) => new SourceApplicationFacetValue(user.UserId, results?.Count ?? 0, user.ShortName ?? user.Name))
            .OrderBy(r => r.Name)
            .ToArray();

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.TrnRequests.Index(Search, SortBy, SortDirection, pageNumber));

        ShowFilters = ResultsBySourceApplication.Any(a => a.Count > 0);

        return Page();
    }

    public record SourceApplicationFacetValue(Guid ApplicationUserId, int Count, string Name);
}
