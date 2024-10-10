using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public class EditMqStartDateState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditMqStartDate,
        typeof(EditMqStartDateState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public MqChangeStartDateReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(StartDate), nameof(ChangeReason), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => StartDate is not null &&
        ChangeReason.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        StartDate = CurrentStartDate = qualificationInfo.MandatoryQualification.StartDate;
        Initialized = true;
    }
}
