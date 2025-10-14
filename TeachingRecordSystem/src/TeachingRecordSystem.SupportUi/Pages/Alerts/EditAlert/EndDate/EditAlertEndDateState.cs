using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

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

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(EndDate), nameof(ChangeReason), nameof(HasAdditionalReasonDetail))]
    public bool IsComplete => EndDate is not null &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail is bool hasDetail &&
        (!hasDetail || ChangeReasonDetail is not null) &&
        Evidence.IsComplete;

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
