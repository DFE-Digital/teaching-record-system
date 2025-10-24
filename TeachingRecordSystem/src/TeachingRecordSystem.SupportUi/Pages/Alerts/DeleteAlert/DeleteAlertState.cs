using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DeleteAlert,
        typeof(DeleteAlertState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public DeleteAlertReasonOption? DeleteReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? DeleteReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(HasAdditionalReasonDetail))]
    public bool IsComplete =>
        DeleteReason is not null &&
        HasAdditionalReasonDetail is bool hasDetail &&
        (!hasDetail || DeleteReasonDetail is not null) &&
        Evidence.IsComplete;

}
