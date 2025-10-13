using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

public class ReopenAlertState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.ReopenAlert,
        typeof(ReopenAlertState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public ReopenAlertReasonOption? ChangeReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ChangeReason), nameof(HasAdditionalReasonDetail))]
    public bool IsComplete =>
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail is bool hasDetail &&
        (!hasDetail || ChangeReasonDetail is not null) &&
        Evidence.IsComplete;
}
