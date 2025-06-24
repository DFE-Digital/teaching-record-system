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
            IntegrationTransactionRecordSortByOption.Name => r => r.Person!.FirstName + " " + r.Person!.LastName,
            _ => r => r.Status
        };

        var sortedRecords = records.OrderBy(sortDirection, sortExpr);

        IntegrationTransaction = new ItDetailResult(
            IntegrationTransactionId: integrationTransaction.IntegrationTransactionId,
            InterfaceType: integrationTransaction.InterfaceType,
            CreatedOn: integrationTransaction.CreatedDate,
            ImportStatus: integrationTransaction.ImportStatus,
            TotalCount: records.Count(),
            SuccessesCount: records.Count(r => r.Status == IntegrationTransactionRecordStatus.Success),
            FailuresCount: records.Count(r => r.Status == IntegrationTransactionRecordStatus.Failure),
            DuplicatesCount: records.Count(r => r.Duplicate == true),
            FileName: integrationTransaction.FileName
        );

        IntegrationTransactionRecords = sortedRecords
            .Select(r => new ItrRow(
                r.IntegrationTransactionRecordId,
                r.PersonId,
                $"{r.Person!.FirstName} {r.Person.LastName}",
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

        //first row in rowdata is the header
        var header = records.FirstOrDefault();
        if (header != null)
        {
            using var stringReader = new StringReader(header.RowData!);
            var headerStr = stringReader.ReadLine();

            if (headerStr != null)
            {
                var headers = headerStr.Split(',');
                foreach (var h in headers)
                {
                    csv.WriteField(h);
                }
                csv.NextRecord();
            }
        }

        //write rows
        foreach (var record in records)
        {
            using var stringReader = new StringReader(record.RowData!);

            // Skip header
            var _ = stringReader.ReadLine();
            var dataLine = stringReader.ReadLine();

            if (!string.IsNullOrWhiteSpace(dataLine))
            {
                var rowFields = dataLine.Split(',');
                foreach (var field in rowFields)
                {
                    csv.WriteField(field.Trim());
                }
                csv.WriteField(record.FailureMessage); // add failure message column
                csv.NextRecord();
            }
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
    Guid ContactId,
    string ContactName,
    bool? Duplicate,
    IntegrationTransactionRecordStatus Status);

