using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusPostRequestContentBuilder : PostRequestContentBuilder
{
    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? UploadedEvidenceFileUrl { get; set; }

    public SetStatusPostRequestContentBuilder WithDeactivateReason(DeactivateReasonOption deactivateReason, string? detail = null)
    {
        DeactivateReason = deactivateReason;
        DeactivateReasonDetail = detail;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithReactivateReason(ReactivateReasonOption reactivateReason, string? detail = null)
    {
        ReactivateReason = reactivateReason;
        ReactivateReasonDetail = detail;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsEvidenceFileName, string? otherDetailsEvidenceFileSizeDescription, string? otherDetailsUploadedEvidenceFileUrl)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFileId = evidenceFileId;
        EvidenceFileName = otherDetailsEvidenceFileName;
        EvidenceFileSizeDescription = otherDetailsEvidenceFileSizeDescription;
        UploadedEvidenceFileUrl = otherDetailsUploadedEvidenceFileUrl;
        return this;
    }
}
