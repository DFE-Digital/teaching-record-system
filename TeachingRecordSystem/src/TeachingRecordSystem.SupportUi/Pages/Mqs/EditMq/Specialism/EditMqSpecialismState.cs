using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public class EditMqSpecialismState
{
    public bool Initialized { get; set; }

    public MandatoryQualificationSpecialism? CurrentSpecialism { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MqChangeSpecialismReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Specialism), nameof(ChangeReason), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => Specialism is not null &&
        ChangeReason.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        Specialism = CurrentSpecialism = qualificationInfo.MandatoryQualification.Specialism;
        Initialized = true;
    }
}
