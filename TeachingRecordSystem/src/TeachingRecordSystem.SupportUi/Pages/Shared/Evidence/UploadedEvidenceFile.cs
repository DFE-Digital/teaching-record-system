namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class UploadedEvidenceFile
{
    public required Guid FileId { get; set; }
    public required string FileName { get; set; }
    public required string FileSizeDescription { get; set; }
    public string? PreviewUrl { get; set; }

    public EventModels.File ToEventModel() => new()
    {
        FileId = FileId,
        Name = FileName
    };
}
