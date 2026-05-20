using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

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

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(StartDate), nameof(ChangeReason))]
    public bool IsComplete => StartDate is not null &&
        ChangeReason.HasValue &&
        Evidence.IsComplete;

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
