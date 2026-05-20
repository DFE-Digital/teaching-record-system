using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public class EditMqSpecialismState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditMqSpecialism,
        typeof(EditMqSpecialismState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public MandatoryQualificationSpecialism? CurrentSpecialism { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MqChangeSpecialismReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Specialism), nameof(ChangeReason))]
    public bool IsComplete => Specialism is not null &&
        ChangeReason.HasValue &&
        Evidence.IsComplete;

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
