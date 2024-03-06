using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class AddMqState
{
    public Guid? ProviderId { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public DateOnly? StartDate { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? EndDate { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ProviderId), nameof(Specialism), nameof(StartDate), nameof(Status))]
    public bool IsComplete =>
        ProviderId.HasValue &&
        Specialism.HasValue &&
        StartDate.HasValue &&
        Status.HasValue &&
        (Status != MandatoryQualificationStatus.Passed || (Status == MandatoryQualificationStatus.Passed && EndDate.HasValue));
}
