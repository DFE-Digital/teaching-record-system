namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Action
{
    public static Guid DqtActionUserId { get; } = Guid.Empty;

    public required Guid ActionId { get; init; }
    public required ActionType ActionType { get; init; }
    public required DateTime Created { get; init; }
    public required Guid UserId { get; init; }
    public ICollection<ActionEvent>? Events { get; }
    public required Guid[] PersonIds { get; init; }
}
