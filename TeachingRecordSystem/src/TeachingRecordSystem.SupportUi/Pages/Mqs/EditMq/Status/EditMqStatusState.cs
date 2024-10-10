using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public class EditMqStatusState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditMqStatus,
        typeof(EditMqStatusState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public MandatoryQualificationStatus? CurrentStatus { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public MqChangeStatusReasonOption? StatusChangeReason { get; set; }

    public MqChangeEndDateReasonOption? EndDateChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    public bool IsEndDateChange => EndDate != CurrentEndDate;

    [JsonIgnore]
    public bool IsStatusChange => Status != CurrentStatus;

    [JsonIgnore]
    public bool IsComplete => Status.HasValue &&
        (Status != MandatoryQualificationStatus.Passed || Status == MandatoryQualificationStatus.Passed && EndDate.HasValue) &&
        (StatusChangeReason.HasValue || EndDateChangeReason.HasValue) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        Status = CurrentStatus = qualificationInfo.MandatoryQualification.Status;
        EndDate = CurrentEndDate = qualificationInfo.MandatoryQualification.EndDate;
        Initialized = true;
    }
}
