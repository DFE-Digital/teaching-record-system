using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

public class EditAlertEndDateState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditAlertEndDate,
        typeof(EditAlertEndDateState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public AlertChangeEndDateReasonOption? ChangeReason { get; set; }

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

    public void EnsureInitialized(CurrentAlertFeature alertInfo)
    {
        if (Initialized)
        {
            return;
        }

        EndDate = CurrentEndDate = alertInfo.Alert.EndDate;
        Initialized = true;
    }
}
