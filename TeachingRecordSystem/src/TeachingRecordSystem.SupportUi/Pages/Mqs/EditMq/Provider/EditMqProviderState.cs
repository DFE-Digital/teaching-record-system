using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public class EditMqProviderState
{
    public bool Initialized { get; set; }

    public string? CurrentMqEstablishmentName { get; set; }

    public string? MqEstablishmentValue { get; set; }

    public MqChangeProviderReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(MqEstablishmentValue), nameof(ChangeReason), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => !string.IsNullOrWhiteSpace(MqEstablishmentValue) &&
        ChangeReason.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        CurrentMqEstablishmentName = qualificationInfo.DqtEstablishmentName;
        MqEstablishmentValue = qualificationInfo.DqtEstablishmentValue;
        Initialized = true;
    }
}
