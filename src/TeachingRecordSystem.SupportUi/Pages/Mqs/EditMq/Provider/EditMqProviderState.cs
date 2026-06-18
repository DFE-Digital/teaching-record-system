using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public class EditMqProviderState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditMqProvider,
        typeof(EditMqProviderState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public Guid? CurrentProviderId { get; set; }

    public Guid? ProviderId { get; set; }

    public MqChangeProviderReasonOption? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public string? AdditionalInformation { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ProviderId), nameof(ChangeReason))]
    public bool IsComplete =>
        ProviderId.HasValue &&
        ChangeReason.HasValue &&
        (ChangeReason == MqChangeProviderReasonOption.AnotherReason && !string.IsNullOrEmpty(ChangeReasonDetail) || (ChangeReason != MqChangeProviderReasonOption.AnotherReason && string.IsNullOrEmpty(ChangeReasonDetail))) &&
        (ProvideAdditionalInformation == true && !string.IsNullOrEmpty(AdditionalInformation) || (ProvideAdditionalInformation != true && string.IsNullOrEmpty(AdditionalInformation))) &&
        Evidence.IsComplete;

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        ProviderId = CurrentProviderId = qualificationInfo.MandatoryQualification.ProviderId;
        Initialized = true;
    }
}
