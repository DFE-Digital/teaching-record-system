using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionPostRequestContentBuilder : PostRequestContentBuilder
{
    private DateOnly? StartDate { get; set; }
    private DateOnly? CompletedDate { get; set; }
    private InductionStatus? InductionStatus { get; set; }
    private Guid[]? ExemptionReasonIds { get; set; }
    private InductionChangeReasonOption? ChangeReason { get; set; }
    private bool? HasAdditionalReasonDetail { get; set; }
    private string? ChangeReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }
    private (HttpContent, string)? EvidenceFile { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private string? EvidenceFileSizeDescription { get; set; }
    private string? UploadedEvidenceFileUrl { get; set; }

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

    //public EditInductionPostRequestContentBuilder WithNoFileUploadSelection()
    //{
    //    UploadEvidence = false;
    //    return this;
    //}

    //public EditInductionPostRequestContentBuilder WithFileUploadSelection(HttpContent binaryFileContent, string filename)
    //{
    //    UploadEvidence = true;
    //    EvidenceFile = (binaryFileContent, filename);
    //    return this;
    //}

    public EditInductionPostRequestContentBuilder WithEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription, string? uploadedEvidenceFileUrl)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFileId = evidenceFileId;
        EvidenceFileName = evidenceFileName;
        EvidenceFileSizeDescription = evidenceFileSizeDescription;
        UploadedEvidenceFileUrl = uploadedEvidenceFileUrl;
        return this;
    }
}
