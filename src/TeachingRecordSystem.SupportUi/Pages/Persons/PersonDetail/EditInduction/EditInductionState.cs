using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionState
{
    public InductionStatus InductionStatus { get; set; }

    // The status the person holds now, which decides whether the status question warns that
    // induction is managed by CPD.
    public InductionStatus CurrentInductionStatus { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public Guid[] ExemptionReasonIds { get; set; } = [];

    public PersonInductionChangeReason? ChangeReason { get; set; }

    public bool? ProvideAdditionalInformation { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();
}
