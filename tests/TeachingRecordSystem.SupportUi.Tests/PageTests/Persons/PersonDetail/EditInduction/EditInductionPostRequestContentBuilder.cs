using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionPostRequestContentBuilder : PostRequestContentBuilder
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public InductionStatus? InductionStatus { get; set; }
    public Guid[]? ExemptionReasonIds { get; set; }
    public PersonInductionChangeReason? ChangeReason { get; set; }
    public bool? ProvideAdditionalInformation { get; set; }
    public string? AdditionalInformation { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public TestEvidenceUploadModel Evidence { get; set; } = new();

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

    public EditInductionPostRequestContentBuilder WithChangeReason(PersonInductionChangeReason changeReason, string? changeReasonDetail = null)
    {
        this.ChangeReason = changeReason;
        ChangeReasonDetail = changeReasonDetail;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithProvideAdditionalInformation(bool? provideAdditionalInformation, string? additionalInformation = null)
    {
        ProvideAdditionalInformation = provideAdditionalInformation;
        AdditionalInformation = additionalInformation;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        Evidence.UploadEvidence = uploadEvidence;
        Evidence.EvidenceFile = evidenceFile;
        return this;
    }

    public EditInductionPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription)
    {
        Evidence.UploadEvidence = uploadEvidence;
        Evidence.UploadedEvidenceFile = evidenceFileId is not Guid id ? null : new()
        {
            FileId = id,
            FileName = evidenceFileName ?? "filename.jpg",
            FileSizeDescription = evidenceFileSizeDescription ?? "5 MB"
        };
        return this;
    }
}
