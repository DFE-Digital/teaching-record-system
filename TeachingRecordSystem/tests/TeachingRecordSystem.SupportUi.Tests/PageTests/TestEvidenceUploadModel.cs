using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests;

public class TestEvidenceUploadModel
{
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }
}
