using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

[Authorize(AuthorizationPolicies.AdminOnly)]
public class Events(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public IReadOnlyCollection<ProcessEventPayload>? Processes { get; set; }

    public async Task OnGetAsync()
    {
        var processesAndEvents = await dbContext.Processes
            .Where(p => p.SupportTaskReferences.Contains(SupportTaskReference))
            .Include(p => p.User)
            .Include(p => p.Events).AsSplitQuery()
            .ToArrayAsync();

        Processes = processesAndEvents
            .Select(p => new ProcessEventPayload(
                p.ProcessType.ToString(),
                p.Events!.Select(e => new ProcessEventPayloadEvent(e.EventName, e.Payload)).ToArray(),
                p.ChangeReason,
                p.CreatedOn))
            .ToArray();
    }

    public record ProcessEventPayload(string ProcessType, IReadOnlyCollection<ProcessEventPayloadEvent> Events, IChangeReasonInfo? ChangeReason, DateTime Timestamp)
        : EventsModel.IEventEntry
    {
        public string? GetFormattedChangeReasonJsonPayload()
        {
            if (ChangeReason is null)
            {
                return null;
            }

            var jsonElement = JsonSerializer.SerializeToElement(ChangeReason, IChangeReasonInfo.SerializerOptions);
            return JsonSerializer.Serialize(jsonElement, EventsModel.IEventEntry.SerializerOptions);
        }
    }

    public record ProcessEventPayloadEvent(string EventName, IEvent Payload)
    {
        public string GetFormattedJsonPayload()
        {
            var jsonElement = JsonSerializer.SerializeToElement(Payload, IEvent.SerializerOptions);
            return JsonSerializer.Serialize(jsonElement, EventsModel.IEventEntry.SerializerOptions);
        }
    }
}
