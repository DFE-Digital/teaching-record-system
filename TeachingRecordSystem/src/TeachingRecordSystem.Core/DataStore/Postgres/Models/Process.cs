namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Process
{
    public required Guid ProcessId { get; init; }
    public required ProcessType ProcessType { get; init; }
    public required DateTime Created { get; init; }
    public required Guid? UserId { get; init; }
    public UserBase? User { get; }
    public required Guid? DqtUserId { get; init; }
    public required string? DqtUserName { get; init; }
    public ICollection<ProcessEvent>? Events { get; }
    public required List<Guid> PersonIds { get; init; }
}
