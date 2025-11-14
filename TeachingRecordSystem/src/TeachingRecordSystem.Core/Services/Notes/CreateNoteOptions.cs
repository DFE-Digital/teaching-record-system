namespace TeachingRecordSystem.Core.Services.Notes;

public record CreateNoteOptions
{
    public required Guid PersonId { get; init; }
    public required string Content { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required Guid? FileId { get; init; }
    public required string? OriginalFileName { get; init; }
}
