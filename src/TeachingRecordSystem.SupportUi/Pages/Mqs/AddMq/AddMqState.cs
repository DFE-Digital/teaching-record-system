using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class AddMqState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.AddMq,
        typeof(AddMqState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public Guid? ProviderId { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public DateOnly? StartDate { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public AddMqReasonOption? AddReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? AddReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    public string? AdditionalInformation { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ProviderId), nameof(Specialism), nameof(StartDate), nameof(Status))]
    public bool IsComplete =>
        ProviderId.HasValue &&
        Specialism.HasValue &&
        StartDate.HasValue &&
        Status.HasValue &&
        (Status != MandatoryQualificationStatus.Passed ||
         (Status == MandatoryQualificationStatus.Passed && EndDate.HasValue)) &&
        AddReason.HasValue &&
        (AddReason == AddMqReasonOption.AnotherReason && !string.IsNullOrEmpty(AddReasonDetail) ||
         AddReason != AddMqReasonOption.AnotherReason && string.IsNullOrEmpty(AddReasonDetail)) &&
         ProvideAdditionalInformation is bool proveAdditionalInfo &&
         (proveAdditionalInfo == true && !string.IsNullOrEmpty(AdditionalInformation) ||
          proveAdditionalInfo == false && string.IsNullOrEmpty(AdditionalInformation)) &&
         Evidence.IsComplete;
}
