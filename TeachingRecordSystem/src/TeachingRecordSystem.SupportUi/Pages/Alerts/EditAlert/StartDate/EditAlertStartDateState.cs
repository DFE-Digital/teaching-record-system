using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

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

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(StartDate), nameof(ChangeReason), nameof(HasAdditionalReasonDetail))]
    public bool IsComplete => StartDate is not null && StartDate != CurrentStartDate &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        Evidence.IsComplete;

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
