using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusPostRequestContentBuilder : PostRequestContentBuilder
{
    public PersonDeactivateReason? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public PersonReactivateReason? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public TestEvidenceUploadModel Evidence { get; set; } = new();

    public SetStatusPostRequestContentBuilder WithDeactivateReason(PersonDeactivateReason deactivateReason, string? detail = null)
    {
        DeactivateReason = deactivateReason;
        DeactivateReasonDetail = detail;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithReactivateReason(PersonReactivateReason reactivateReason, string? detail = null)
    {
        ReactivateReason = reactivateReason;
        ReactivateReasonDetail = detail;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        Evidence.UploadEvidence = uploadEvidence;
        Evidence.EvidenceFile = evidenceFile;
        return this;
    }

    public SetStatusPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription)
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
