using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertState
{
    public Guid? AlertTypeId { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(AlertTypeId), nameof(Details), nameof(StartDate))]
    public bool IsComplete => AlertTypeId.HasValue && !string.IsNullOrWhiteSpace(Details) && StartDate.HasValue;
}
