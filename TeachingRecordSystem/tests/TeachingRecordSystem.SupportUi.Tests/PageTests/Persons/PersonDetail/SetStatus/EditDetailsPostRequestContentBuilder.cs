using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusPostRequestContentBuilder : PostRequestContentBuilder
{
    private DeactivateReasonOption? DeactivateReason { get; set; }
    private string? DeactivateReasonDetail { get; set; }
    private ReactivateReasonOption? ReactivateReason { get; set; }
    private string? ReactivateReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }
    private (HttpContent, string)? EvidenceFile { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private string? EvidenceFileSizeDescription { get; set; }
    private string? UploadedEvidenceFileUrl { get; set; }

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
