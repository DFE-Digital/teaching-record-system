using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;

public class DetailModel(TrsDbContext context) : PageModel
{
    [FromRoute]
    public long IntegrationTransactionId { get; set; }

    public ItDetailResult? IntegrationTransaction { get; set; }

    public List<ItrRow> IntegrationTransactionRecords { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public IntegrationTransactionRecordSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]

    public int? PageNumber { get; set; }

    public bool HasFailures { get; set; }


    public async Task<IActionResult> OnGetAsync()
    {
        var sortBy = SortBy ??= IntegrationTransactionRecordSortByOption.Status;
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Descending;

        var integrationTransaction = await context.IntegrationTransactions
            .Include(x => x.IntegrationTransactionRecords!)
                .ThenInclude(r => r.Person)
                .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.IntegrationTransactionId == IntegrationTransactionId);

        if (integrationTransaction is null)
        {
            return NotFound();
        }

        var records = integrationTransaction.IntegrationTransactionRecords!.AsQueryable();

        Expression<Func<IntegrationTransactionRecord, object>> sortExpr = sortBy switch
        {
            IntegrationTransactionRecordSortByOption.IntegrationTransactionRecordId => r => r.IntegrationTransactionRecordId,
            IntegrationTransactionRecordSortByOption.Duplicate => r => r.Duplicate ?? false,
            IntegrationTransactionRecordSortByOption.Status => r => r.Status,
            IntegrationTransactionRecordSortByOption.Name => r => r.Person != null
                ? r.Person.FirstName + " " + r.Person.LastName
                : "Unknown",
            _ => r => r.Status
        };

        var sortedRecords = records.OrderBy(sortDirection, sortExpr);

        IntegrationTransaction = new ItDetailResult(
            IntegrationTransactionId: integrationTransaction.IntegrationTransactionId,
            InterfaceType: integrationTransaction.InterfaceType,
            CreatedOn: integrationTransaction.CreatedDate,
            ImportStatus: integrationTransaction.ImportStatus,
            TotalCount: integrationTransaction.TotalCount,
            SuccessesCount: integrationTransaction.SuccessCount,
            FailuresCount: integrationTransaction.FailureCount,
            DuplicatesCount: integrationTransaction.DuplicateCount,
            FileName: integrationTransaction.FileName
        );

        IntegrationTransactionRecords = sortedRecords
            .Select(r => new ItrRow(
                r.IntegrationTransactionRecordId,
                r.PersonId,
                r.Person != null
                    ? $"{r.Person.FirstName} {r.Person.LastName}"
                    : "Unknown",
                r.Duplicate,
                r.Status
            ))
            .ToList();

        HasFailures = IntegrationTransactionRecords.Any(r => r.Status == IntegrationTransactionRecordStatus.Failure);


        return Page();
    }

    public async Task<IActionResult> OnGetDownloadFailuresAsync()
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        var integrationTransaction = await context.IntegrationTransactions
            .Include(x => x.IntegrationTransactionRecords!)
                .ThenInclude(r => r.Person)
            .SingleOrDefaultAsync(x => x.IntegrationTransactionId == IntegrationTransactionId);

        if (integrationTransaction?.IntegrationTransactionRecords == null)
        {
            return NotFound();
        }

        var records = integrationTransaction.IntegrationTransactionRecords
            .Where(x => x.Status == IntegrationTransactionRecordStatus.Failure)
            .OrderBy(x => x.IntegrationTransactionId);


        // Some files contain a header but other files do not.
        // also, some files are command delimited, others are semicolon
        foreach (var record in records)
        {
            csv.WriteField(record.RowData);
            csv.WriteField(record.FailureMessage);
            csv.NextRecord();
        }

        var csvContent = writer.ToString();
        var bytes = Encoding.UTF8.GetBytes(csvContent);

        return File(bytes, "text/csv", $"{IntegrationTransactionId}.{integrationTransaction.InterfaceType}.failures.csv");
    }
}


public record ItDetailResult(
    long IntegrationTransactionId,
    IntegrationTransactionInterfaceType InterfaceType,
    DateTime CreatedOn,
    IntegrationTransactionImportStatus ImportStatus,
    int TotalCount,
    int SuccessesCount,
    int FailuresCount,
    int DuplicatesCount,
    string FileName);


public record ItrRow(
    long IntegrationTransactionRecordId,
    Guid? PersonId,
    string PersonName,
    bool? Duplicate,
    IntegrationTransactionRecordStatus Status);

