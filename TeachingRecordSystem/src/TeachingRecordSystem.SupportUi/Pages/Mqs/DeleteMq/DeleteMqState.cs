using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public class DeleteMqState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DeleteMq,
        typeof(DeleteMqState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(DeletionReason), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => DeletionReason.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));
}
