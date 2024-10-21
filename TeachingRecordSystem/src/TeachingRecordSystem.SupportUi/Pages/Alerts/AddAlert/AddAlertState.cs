using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(AlertTypeId), nameof(Details), nameof(StartDate), nameof(UploadEvidence))]
    public bool IsComplete =>
        AlertTypeId.HasValue &&
        !string.IsNullOrWhiteSpace(Details) &&
        AddLink.HasValue &&
        StartDate.HasValue &&
        AddReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));
}
