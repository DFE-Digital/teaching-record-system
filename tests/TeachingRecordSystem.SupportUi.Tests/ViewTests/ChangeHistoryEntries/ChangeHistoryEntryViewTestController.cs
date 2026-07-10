using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries;

public class ChangeHistoryEntryViewTestController(TrsDbContext dbContext) : Controller
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("_change-history-entry/{processId:guid}")]
    public async Task<IActionResult> ChangeHistoryEntry(Guid processId)
    {
        var process = await dbContext.Processes
            .Include(p => p.Events)
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.ProcessId == processId);

        if (process is null)
        {
            return NotFound();
        }

        var vm = CreateViewModel(process);

        return View("/ViewTests/ChangeHistoryEntries/ChangeHistoryEntry.cshtml", vm);
    }

    [HttpPost("_change-history-entry")]
    public async Task<IActionResult> ChangeHistoryEntry([FromBody] ChangeHistoryEntryRequest request)
    {
        var process = await dbContext.Processes
            .Include(p => p.Events)
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.ProcessId == request.ProcessId);

        if (process is null)
        {
            return NotFound();
        }

        var vm = CreateViewModel(process, request.ModelProperties);

        return View("/ViewTests/ChangeHistoryEntries/ChangeHistoryEntry.cshtml", vm);
    }

    private static ChangeHistoryEntryViewModel CreateViewModel(
        TeachingRecordSystem.Core.DataStore.Postgres.Models.Process process,
        IReadOnlyDictionary<string, JsonElement>? modelProperties = null)
    {
        var viewModel = new ChangeHistoryEntryViewModel
        {
            ViewType = ChangeHistoryViewType.Person,
            Timestamp = process.CreatedOn,
            UserName = process.User?.Name ?? process.DqtUserName!,
            ProcessId = process.ProcessId,
            ProcessType = process.ProcessType,
            ChangeReason = process.ChangeReason,
            Events = process.Events!.Select(e => e.Payload).AsReadOnly(),
            PersonInfo = null,
            PersonId = null
        };

        if (modelProperties is null)
        {
            return viewModel;
        }

        foreach (var (propertyName, propertyValue) in modelProperties)
        {
            var property = typeof(ChangeHistoryEntryViewModel).GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (property is null || property.SetMethod is null)
            {
                throw new InvalidOperationException($"Unknown change history entry model property '{propertyName}'.");
            }

            var value = propertyValue.Deserialize(property.PropertyType, JsonSerializerOptions);
            property.SetValue(viewModel, value);
        }

        return viewModel;
    }

    public sealed record ChangeHistoryEntryRequest(
        Guid ProcessId,
        IReadOnlyDictionary<string, JsonElement>? ModelProperties);
}
