using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Pages.Routes;

public class ChangeReasonState(IFileService fileService)
{
    public ChangeReasonOption? ChangeReason;
    public string? ChangeReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public async Task<string?> GetEvidenceFileUrlAsync()
    {
        return EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(EvidenceFileId!.Value, InductionDefaults.FileUrlExpiry) : // CML TODO - move to a general defaults file
            null;
    }
}
