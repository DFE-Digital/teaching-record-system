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
    [FromRoute]
    public Guid PersonId { get; set; }

    public IReadOnlyCollection<IEventEntry>? EventPayloads { get; set; }

    public async Task OnGetAsync()
    {
        var legacyEvents = await dbContext.Events
            .Where(e => e.PersonId == PersonId)
            .ToArrayAsync();

        var processes = await dbContext.Processes
            .Where(p => p.PersonIds.Contains(PersonId))
            .Include(p => p.User)
            .Include(p => p.Events).AsSplitQuery()
            .ToArrayAsync();

        EventPayloads = legacyEvents
            .Select(e => (IEventEntry)new LegacyEventPayload(e.EventName, e.Payload, e.Created))
            .Concat(
                processes
                    .Select(p => new ProcessEventPayload(
                        p.ProcessType.ToString(),
                        p.Events!.OrderBy(e => e.CreatedOn).Select(x => new ProcessEventPayloadEvent(x.EventName, x.Payload)).ToArray(),
                        p.ChangeReason,
                        p.CreatedOn)))
            .OrderBy(e => e.Timestamp)
            .ToArray();
    }

    public interface IEventEntry
    {
        public static JsonSerializerOptions SerializerOptions { get; } = new() { WriteIndented = true };

        DateTime Timestamp { get; }
    }

    public record LegacyEventPayload(string EventName, string Payload, DateTime Timestamp) : IEventEntry
    {
        public string GetFormattedJsonPayload()
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(Payload);
            return JsonSerializer.Serialize(jsonElement, IEventEntry.SerializerOptions);
        }
    }

    public record ProcessEventPayload(string ProcessType, IReadOnlyCollection<ProcessEventPayloadEvent> Events, IChangeReasonInfo? ChangeReason, DateTime Timestamp)
        : IEventEntry
    {
        public string? GetFormattedChangeReasonJsonPayload()
        {
            if (ChangeReason is null)
            {
                return null;
            }

            var jsonElement = JsonSerializer.SerializeToElement(ChangeReason, IChangeReasonInfo.SerializerOptions);
            return JsonSerializer.Serialize(jsonElement, IEventEntry.SerializerOptions);
        }
    }

    public record ProcessEventPayloadEvent(string EventName, IEvent Payload)
    {
        public string GetFormattedJsonPayload()
        {
            var jsonElement = JsonSerializer.SerializeToElement(Payload, IEvent.SerializerOptions);
            return JsonSerializer.Serialize(jsonElement, IEventEntry.SerializerOptions);
        }
    }
}
