using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(EndDate), nameof(ChangeReason), nameof(HasAdditionalReasonDetail), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => EndDate is not null &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        (!HasAdditionalReasonDetail.Value || (HasAdditionalReasonDetail.Value && !string.IsNullOrWhiteSpace(ChangeReasonDetail))) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));
}
