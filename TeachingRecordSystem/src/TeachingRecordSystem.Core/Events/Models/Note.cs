namespace TeachingRecordSystem.Core.Events.Models;

public record Note
{
    public required Guid NoteId { get; init; }
    public required Guid PersonId { get; init; }
    public required string Content { get; init; }
    public required File? File { get; init; }

    public static Note FromModel(Core.DataStore.Postgres.Models.Note model) =>
        new()
        {
            NoteId = model.NoteId,
            PersonId = model.PersonId,
            Content = model.Content!,
            File = model.FileId is not null ?
                new File
                {
                    FileId = model.FileId!.Value,
                    Name = model.OriginalFileName!
                } :
                null
        };
}
