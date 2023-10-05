using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public class CloseAlertState
{
    public DateOnly? EndDate { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(EndDate))]
    public bool IsComplete => EndDate.HasValue;
}
