using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;

public class IndexModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
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

        if (sortBy == IntegrationTransactionSortByOption.CreatedOn)
        {
            query = query.OrderBy(sortDirection, t => t.CreatedDate!).ThenBy(x => x.ImportStatus);

        }
        else if (sortBy == IntegrationTransactionSortByOption.Duplicates)
        {
            query = query.OrderBy(sortDirection, t => t.DuplicateCount!);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Failures)
        {
            query = query.OrderBy(sortDirection, t => t.FailureCount!);
        }
        else if (sortBy == IntegrationTransactionSortByOption.ImportStatus)
        {
            query = query.OrderBy(sortDirection, t => t.ImportStatus!);
        }
        else if (sortBy == IntegrationTransactionSortByOption.IntegrationTransactionId)
        {
            query = query.OrderBy(sortDirection, t => t.IntegrationTransactionId!);
        }
        else if (sortBy == IntegrationTransactionSortByOption.InterfaceType)
        {
            query = query.OrderBy(sortDirection, t => t.InterfaceType!);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Total)
        {
            query = query.OrderBy(sortDirection, t => t.TotalCount!);
        }
        else if (sortBy == IntegrationTransactionSortByOption.Successes)
        {
            query = query.OrderBy(sortDirection, t => t.SuccessCount!);
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
                x.FailureCount,
                x.DuplicateCount
            ))
            .GetPageAsync(PageNumber, IntegrationTransactionsPerPage);

        Pagination = PaginationViewModel.Create(
            Results,
            page => linkGenerator.IntegrationTransactions(SortBy, SortDirection, page));
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
    int FailuresCount,
    int DuplicatesCount);
