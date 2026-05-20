using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;

public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int IntegrationTransactionsPerPage = 20;
    public ResultPage<ItResult>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public IntegrationTransactionSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Descending;
        var sortBy = SortBy ??= IntegrationTransactionSortByOption.CreatedOn;

        var query = dbContext.IntegrationTransactions.AsQueryable();
        var totalIntegrationTransactionCount = await query.CountAsync();

        if (sortBy == IntegrationTransactionSortByOption.CreatedOn)
        {
            query = query.OrderBy(t => t.CreatedDate!, sortDirection).ThenBy(x => x.ImportStatus);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Duplicates)
        {
            query = query.OrderBy(t => t.DuplicateCount!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Failures)
        {
            query = query.OrderBy(t => t.FailureCount!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.ImportStatus)
        {
            query = query.OrderBy(t => t.ImportStatus!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.IntegrationTransactionId)
        {
            query = query.OrderBy(t => t.IntegrationTransactionId!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.InterfaceType)
        {
            query = query.OrderBy(t => t.InterfaceType!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Total)
        {
            query = query.OrderBy(t => t.TotalCount!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Successes)
        {
            query = query.OrderBy(t => t.SuccessCount!, sortDirection);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Warnings)
        {
            query = query.OrderBy(t => t.WarningCount!, sortDirection);
        }

        Results = await query
            .Include(x => x.IntegrationTransactionRecords)
            .Select(x => new ItResult(
                x.IntegrationTransactionId,
                x.InterfaceType,
                x.CreatedDate,
                x.ImportStatus,
                x.TotalCount,
                x.SuccessCount,
                x.WarningCount,
                x.FailureCount,
                x.DuplicateCount
            ))
            .GetPageAsync(PageNumber, IntegrationTransactionsPerPage, totalIntegrationTransactionCount);

        Pagination = PaginationViewModel.Create(
            Results,
            page => linkGenerator.SupportTasks.IntegrationTransactions.Index(SortBy, SortDirection, page));
        return Page();
    }
}

public record ItResult(
    long IntegrationTransactionId,
    IntegrationTransactionInterfaceType InterfaceType,
    DateTime CreatedOn,
    IntegrationTransactionImportStatus ImportStatus,
    int TotalCount,
    int SuccessesCount,
    int WarningsCount,
    int FailuresCount,
    int DuplicatesCount);
