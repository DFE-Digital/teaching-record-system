namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTaskNote
{
    public required Guid SupportTaskNoteId { get; init; }
    public required string SupportTaskReference { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public User? CreatedBy { get; }
}
