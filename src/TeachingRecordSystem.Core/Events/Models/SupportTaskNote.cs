namespace TeachingRecordSystem.Core.Events.Models;

public record SupportTaskNote
{
    public required Guid SupportTaskNoteId { get; init; }
    public required string SupportTaskReference { get; init; }
    public required string Content { get; init; }

    public static SupportTaskNote FromModel(DataStore.Postgres.Models.SupportTaskNote model) => new()
    {
        SupportTaskNoteId = model.SupportTaskNoteId,
        SupportTaskReference = model.SupportTaskReference,
        Content = model.Content
    };
}
