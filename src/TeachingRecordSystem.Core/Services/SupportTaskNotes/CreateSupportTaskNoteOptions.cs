namespace TeachingRecordSystem.Core.Services.SupportTaskNotes;

public record CreateSupportTaskNoteOptions
{
    public required string SupportTaskReference { get; init; }
    public required string Content { get; init; }
    public required Guid CreatedByUserId { get; init; }
}
