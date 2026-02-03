using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;

public class RowModel(TrsDbContext context) : PageModel
{
    [FromRoute]
    public long IntegrationTransactionId { get; set; }

    [FromQuery]
    public long IntegrationTransactionRecordId { get; set; }

    public DetailRow? Row { get; set; }

    public Person? Person { get; set; }

    public string? PersonName => Person is not null ? ' '.JoinNonEmpty(Person.FirstName, Person.MiddleName, Person.LastName) : null;

    public async Task<IActionResult> OnGetAsync()
    {
        var integrationTransaction = await context.IntegrationTransactions
            .Include(x => x.IntegrationTransactionRecords!)
                .ThenInclude(r => r.Person)
            .SingleOrDefaultAsync(x => x.IntegrationTransactionId == IntegrationTransactionId);

        var record = integrationTransaction?.IntegrationTransactionRecords?
            .FirstOrDefault(r => r.IntegrationTransactionRecordId == IntegrationTransactionRecordId);

        if (integrationTransaction is null || record is null)
        {
            return NotFound();
        }

        Row = new DetailRow(IntegrationTransactionRecordId, integrationTransaction.ImportStatus, record.Status, record.RowData, record.FailureMessage, integrationTransaction.InterfaceType, record.Duplicate);
        Person = record.Person;

        return Page();
    }
}

public record DetailRow(
    long IntegrationTransactionRecordId,
    IntegrationTransactionImportStatus ImportStatus,
    IntegrationTransactionRecordStatus Status,
    string? RowData,
    string? FailureMessage,
    IntegrationTransactionInterfaceType InterfaceType,
    bool? Duplicate);
