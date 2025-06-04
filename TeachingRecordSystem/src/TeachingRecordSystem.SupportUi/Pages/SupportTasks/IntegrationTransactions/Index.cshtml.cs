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
    [FromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Descending;
        var sortBy = SortBy ??= IntegrationTransactionSortByOption.CreatedOn;

        var query = dbContext.IntegrationTransactions.AsQueryable();

        query = (sortBy, sortDirection) switch
        {
            (IntegrationTransactionSortByOption.IntegrationTransactionId, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.IntegrationTransactionId),
            (IntegrationTransactionSortByOption.IntegrationTransactionId, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.IntegrationTransactionId),

            (IntegrationTransactionSortByOption.Interface, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.InterfaceTypeId),
            (IntegrationTransactionSortByOption.Interface, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.InterfaceTypeId),

            (IntegrationTransactionSortByOption.CreatedOn, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.CreatedDate),
            (IntegrationTransactionSortByOption.CreatedOn, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.CreatedDate),

            (IntegrationTransactionSortByOption.ImportStatus, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.ImportStatus),
            (IntegrationTransactionSortByOption.ImportStatus, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.ImportStatus),

            (IntegrationTransactionSortByOption.Total, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.TotalCount),
            (IntegrationTransactionSortByOption.Total, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.TotalCount),

            (IntegrationTransactionSortByOption.Successes, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.SuccessCount),
            (IntegrationTransactionSortByOption.Successes, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.SuccessCount),

            (IntegrationTransactionSortByOption.Failures, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.FailureCount),
            (IntegrationTransactionSortByOption.Failures, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.FailureCount),

            (IntegrationTransactionSortByOption.Duplicates, SupportUi.SortDirection.Ascending) =>
                query.OrderBy(x => x.DuplicateCount),
            (IntegrationTransactionSortByOption.Duplicates, SupportUi.SortDirection.Descending) =>
                query.OrderByDescending(x => x.DuplicateCount),

            _ => query.OrderByDescending(x => x.CreatedDate)
        };

        Results = await query
            .Select(x => new ItResult(
                x.IntegrationTransactionId,
                x.InterfaceTypeId,
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
    Int64 IntegrationTransactionId,
    IntegrationTransactionInterfaceType Interface,
    DateTime CreatedOn,
    IntegrationTransactionImportStatus ImportStatus,
    int TotalCount,
    int SuccessesCount,
    int FailuresCount,
    int DuplicatesCount);
