using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertState
{
    public DeleteAlertReasonOption? DeleteReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? DeleteReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ProvideAdditionalInformation))]
    public bool IsComplete =>
        (DeleteReason.HasValue && DeleteReason == DeleteAlertReasonOption.AnotherReason && !string.IsNullOrEmpty(DeleteReasonDetail) ||
         DeleteReason != DeleteAlertReasonOption.AnotherReason && string.IsNullOrEmpty(DeleteReasonDetail)) &&
        (ProvideAdditionalInformation == true && !string.IsNullOrEmpty(AdditionalInformation) ||
         ProvideAdditionalInformation == false && string.IsNullOrEmpty(AdditionalInformation)) &&
        Evidence.IsComplete;

}
