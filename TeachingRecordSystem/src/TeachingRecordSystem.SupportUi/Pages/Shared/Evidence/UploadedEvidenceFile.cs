using System.Diagnostics.CodeAnalysis;
using Humanizer;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class UploadedEvidenceFile
{
    public UploadedEvidenceFile()
    {
    }

    [SetsRequiredMembers]
    public UploadedEvidenceFile(Guid fileId, string fileName, string? fileSizeDescription = null)
    {
        FileId = fileId;
        FileName = fileName;
        FileSizeDescription = fileSizeDescription;
    }

    [SetsRequiredMembers]
    public UploadedEvidenceFile(Guid fileId, string fileName, long fileSizeBytes)
    {
        FileId = fileId;
        FileName = fileName;
        FileSizeDescription = fileSizeBytes.Bytes().Humanize();
    }

    public required Guid FileId { get; set; }
    public required string FileName { get; set; }
    public string? FileSizeDescription { get; set; }
    public string? PreviewUrl { get; set; }

    public EventModels.File ToEventModel() => new()
    {
        FileId = FileId,
        Name = FileName
    };

    public Core.Services.Persons.File ToFile() => new()
    {
        FileId = FileId,
        Name = FileName
    };
}
