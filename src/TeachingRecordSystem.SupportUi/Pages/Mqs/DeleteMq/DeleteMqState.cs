using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public class DeleteMqState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DeleteMq,
        typeof(DeleteMqState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(DeletionReason))]

    public bool IsComplete => DeletionReason.HasValue &&
        (DeletionReason == MqDeletionReasonOption.AnotherReason && !string.IsNullOrEmpty(DeletionReasonDetail) || DeletionReason != MqDeletionReasonOption.AnotherReason && string.IsNullOrEmpty(DeletionReasonDetail)) &&
        (ProvideAdditionalInformation == true && !string.IsNullOrEmpty(AdditionalInformation) || ProvideAdditionalInformation == false && string.IsNullOrEmpty(AdditionalInformation)) &&
        Evidence.IsComplete;
}
