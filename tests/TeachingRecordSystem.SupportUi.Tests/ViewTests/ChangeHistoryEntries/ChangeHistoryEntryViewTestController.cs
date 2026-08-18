using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries;

public class ChangeHistoryEntryViewTestController(TrsDbContext dbContext) : Controller
{
    [HttpGet("_change-history-entry/{processId:guid}")]
    public async Task<IActionResult> ChangeHistoryEntry(Guid processId, [FromQuery] Guid? personId, [FromQuery] string? contextType, [FromQuery] string? oneLoginSubject, [FromQuery] string? supportTaskReference)
    {
        var process = await dbContext.Processes
            .Include(p => p.Events)
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.ProcessId == processId);

        if (process is null)
        {
            return NotFound();
        }

        var vm = new ChangeHistoryEntryViewModel
        {
            Context = contextType switch
            {
                "oneLogin" => ChangeHistoryContext.ForOneLoginUser(oneLoginSubject ?? string.Empty),
                "supportTask" => ChangeHistoryContext.ForSupportTask(supportTaskReference ?? string.Empty),
                _ => ChangeHistoryContext.ForPerson(personId ?? Guid.Empty),
            },
            Timestamp = process.CreatedOn,
            UserName = process.User?.Name ?? process.DqtUserName!,
            ProcessId = process.ProcessId,
            ProcessType = process.ProcessType,
            ChangeReason = process.ChangeReason,
            Events = process.Events!.Select(e => e.Payload).AsReadOnly()
        };

        return View("/ViewTests/ChangeHistoryEntries/ChangeHistoryEntry.cshtml", vm);
    }
}
