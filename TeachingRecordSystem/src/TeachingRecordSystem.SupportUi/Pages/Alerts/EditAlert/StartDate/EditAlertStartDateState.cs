using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

public class EditAlertStartDateState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditAlertStartDate,
        typeof(EditAlertStartDateState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public AlertChangeStartDateReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(StartDate), nameof(ChangeReason), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => StartDate is not null &&
        ChangeReason.HasValue &&
        (ChangeReason.Value == AlertChangeStartDateReasonOption.AnotherReason ? !string.IsNullOrWhiteSpace(ChangeReasonDetail) : true) &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public void EnsureInitialized(CurrentAlertFeature alertInfo)
    {
        if (Initialized)
        {
            return;
        }

        StartDate = CurrentStartDate = alertInfo.Alert.StartDate;
        Initialized = true;
    }
}
