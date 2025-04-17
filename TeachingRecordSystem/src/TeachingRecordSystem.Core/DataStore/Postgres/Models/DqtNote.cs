namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class DqtNote
{
    public required Guid Id { get; set; }
    public required Guid PersonId { get; set; }
    public required string NoteText { get; set; }
    public required DateTime? UpdatedOn { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required Guid CreatedByDqtUserId { get; set; }
    public required string CreatedByDqtUserName { get; set; }
    public required Guid? UpdatedByDqtUserId { get; set; }
    public required string? UpdatedByDqtUserName { get; set; }
    public required string? FileName { get; set; }
    public required string? OriginalFileName { get; set; }
}
