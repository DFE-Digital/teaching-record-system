using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertState
{
    public Guid? AlertTypeId { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public bool? AddLink { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    public AddAlertReasonOption? AddReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? AddReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(AlertTypeId), nameof(Details), nameof(StartDate))]
    public bool IsComplete =>
        AlertTypeId.HasValue &&
        AddLink.HasValue &&
        StartDate.HasValue &&
        (AddReason.HasValue && AddReason == AddAlertReasonOption.AnotherReason && !string.IsNullOrEmpty(AddReasonDetail) ||
         AddReason != AddAlertReasonOption.AnotherReason && string.IsNullOrEmpty(AddReasonDetail)) &&
        (ProvideAdditionalInformation == true && !string.IsNullOrEmpty(AdditionalInformation) ||
         ProvideAdditionalInformation == false && string.IsNullOrEmpty(AdditionalInformation)) &&
        Evidence.IsComplete;
}
