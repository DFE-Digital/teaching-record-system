using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ChangeReason), nameof(HasAdditionalReasonDetail), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete =>
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        (!HasAdditionalReasonDetail.Value || (HasAdditionalReasonDetail.Value && !string.IsNullOrWhiteSpace(ChangeReasonDetail))) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));
}
