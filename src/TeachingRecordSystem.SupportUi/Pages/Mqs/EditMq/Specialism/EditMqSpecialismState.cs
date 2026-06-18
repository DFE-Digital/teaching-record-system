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

    public string? AdditionalInformation { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Specialism), nameof(ChangeReason))]
    public bool IsComplete => Specialism is not null &&
                              (ChangeReason == MqChangeSpecialismReasonOption.AnotherReason && !string.IsNullOrEmpty(ChangeReasonDetail) || ChangeReason != MqChangeSpecialismReasonOption.AnotherReason && string.IsNullOrEmpty(ChangeReasonDetail)) &&
                              (ProvideAdditionalInformation == true && !string.IsNullOrWhiteSpace(AdditionalInformation) || ProvideAdditionalInformation != true && string.IsNullOrWhiteSpace(AdditionalInformation)) &&
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
