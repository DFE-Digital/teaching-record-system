using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public class CloseAlertState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.CloseAlert,
        typeof(CloseAlertState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public DateOnly? EndDate { get; set; }

    public CloseAlertReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(EndDate), nameof(ChangeReason), nameof(HasAdditionalReasonDetail))]
    public bool IsComplete => EndDate is not null &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail is bool hasDetail &&
        (!hasDetail || ChangeReasonDetail is not null) &&
        Evidence.IsComplete;
}
