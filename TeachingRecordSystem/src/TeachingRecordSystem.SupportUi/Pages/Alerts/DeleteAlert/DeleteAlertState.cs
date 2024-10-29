using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DeleteAlert,
        typeof(DeleteAlertState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? DeleteReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(HasAdditionalReasonDetail), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete =>
        HasAdditionalReasonDetail.HasValue &&
        (!HasAdditionalReasonDetail.Value || (HasAdditionalReasonDetail.Value && !string.IsNullOrWhiteSpace(DeleteReasonDetail))) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

}
