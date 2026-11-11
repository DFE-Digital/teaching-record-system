using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[Authorize(AuthorizationPolicies.AdminOnly)]
[AllowDeactivatedPerson]
public class EventsModel(TrsDbContext dbContext) : PageModel
{
    public static JsonSerializerOptions SerializerOptions { get; } = new() { WriteIndented = true };

    [FromRoute]
    public Guid PersonId { get; set; }

    public IReadOnlyCollection<ProcessEventPayload>? Processes { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var processesAndEvents = await dbContext.Processes
            .AsSplitQuery()
            .Where(p => p.PersonIds.Contains(PersonId))
            .Include(p => p.User)
            .Include(p => p.Events)
            .ToArrayAsync(cancellationToken);

        Processes = processesAndEvents
            .Select(p => new ProcessEventPayload(
                p.ProcessType.ToString(),
                p.Events!.OrderBy(e => e.CreatedOn).Select(e => new ProcessEventPayloadEvent(e.EventName, e.Payload)).ToArray(),
                p.ChangeReason,
                p.CreatedOn))
            .OrderByDescending(p => p.Timestamp)
            .ToArray();
    }

    public record ProcessEventPayload(string ProcessType, IReadOnlyCollection<ProcessEventPayloadEvent> Events, IChangeReasonInfo? ChangeReason, DateTime Timestamp)
    {
        public string? GetFormattedChangeReasonJsonPayload()
        {
            if (ChangeReason is null)
            {
                return null;
            }

            var jsonElement = JsonSerializer.SerializeToElement(ChangeReason, IChangeReasonInfo.SerializerOptions);
            return JsonSerializer.Serialize(jsonElement, SerializerOptions);
        }
    }

    public record ProcessEventPayloadEvent(string EventName, IEvent Payload)
    {
        public string GetFormattedJsonPayload()
        {
            var jsonElement = JsonSerializer.SerializeToElement(Payload, IEvent.SerializerOptions);
            return JsonSerializer.Serialize(jsonElement, SerializerOptions);
        }
    }
}
