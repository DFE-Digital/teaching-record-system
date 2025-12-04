using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

public class Index(SupportTaskSearchService supportTaskSearchService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public TrnRequestManualChecksSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid[]? Sources { get; set; }

    public int? TotalTaskCount { get; set; }

    public ResultPage<TrnRequestManualChecksSearchResultItem>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public IReadOnlyDictionary<SupportTaskSearchFacet, IReadOnlyDictionary<object, int>>? Facets { get; set; }

    [BindProperty(SupportsGet = true, Name = "_f")]
    public bool FormSubmitted { get; set; }

    public async Task OnGetAsync()
    {
        var searchOptions = new TrnRequestManualChecksSearchOptions(Search, SortBy, SortDirection, FormSubmitted, Sources);
        var paginationOptions = new PaginationOptions(PageNumber, TasksPerPage);

        var result = await supportTaskSearchService.SearchTrnRequestManualChecksAsync(searchOptions, paginationOptions);
        TotalTaskCount = result.TotalTaskCount;
        Sources = result.Sources;
        Facets = result.Facets;
        Results = result.SearchResults;

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.TrnRequestManualChecksNeeded.Index(Search, SortBy, SortDirection, pageNumber));
    }

}
