using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusPostRequestContentBuilder : PostRequestContentBuilder
{
    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

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

    public SetStatusPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription)
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
