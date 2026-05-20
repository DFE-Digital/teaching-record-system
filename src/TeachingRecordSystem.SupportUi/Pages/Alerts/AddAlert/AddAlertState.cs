using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.AddAlert,
        typeof(AddAlertState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public Guid? AlertTypeId { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public bool? AddLink { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    public AddAlertReasonOption? AddReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? AddReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(AlertTypeId), nameof(Details), nameof(StartDate))]
    public bool IsComplete =>
        AlertTypeId.HasValue &&
        AddLink.HasValue &&
        StartDate.HasValue &&
        AddReason.HasValue &&
        HasAdditionalReasonDetail is bool hasDetail &&
        (!hasDetail || AddReasonDetail is not null) &&
        Evidence.IsComplete;
}
