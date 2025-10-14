using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionPostRequestContentBuilder : PostRequestContentBuilder
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public InductionStatus? InductionStatus { get; set; }
    public Guid[]? ExemptionReasonIds { get; set; }
    public InductionChangeReasonOption? ChangeReason { get; set; }
    public bool? HasAdditionalReasonDetail { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

    public EditInductionPostRequestContentBuilder WithStartDate(DateOnly date)
    {
        StartDate = date;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithCompletedDate(DateOnly date)
    {
        CompletedDate = date;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithInductionStatus(InductionStatus status)
    {
        InductionStatus = status;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithExemptionReasonIds(Guid[] exemptionReasons)
    {
        ExemptionReasonIds = exemptionReasons;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithChangeReason(InductionChangeReasonOption changeReason)
    {
        this.ChangeReason = changeReason;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithChangeReasonDetailSelections(bool? hasAdditionalDetail, string? detail = null)
    {
        HasAdditionalReasonDetail = hasAdditionalDetail;
        ChangeReasonDetail = detail;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription)
    {
        UploadEvidence = uploadEvidence;
        UploadedEvidenceFile = evidenceFileId is not Guid id ? null : new()
        {
            FileId = id,
            FileName = evidenceFileName ?? "filename.jpg",
            FileSizeDescription = evidenceFileSizeDescription ?? "5 MB"
        };
        return this;
    }
}
