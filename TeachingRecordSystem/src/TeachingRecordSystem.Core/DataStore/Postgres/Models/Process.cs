namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Process
{
    public static Guid DqtProcessUserId { get; } = Guid.Empty;

    public required Guid ProcessId { get; init; }
    public required ProcessType ProcessType { get; init; }
    public required DateTime Created { get; init; }
    public required Guid UserId { get; init; }
    public ICollection<ProcessEvent>? Events { get; }
    public required Guid[] PersonIds { get; init; }
}
