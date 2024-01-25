namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class JourneyState
{
    public required string InstanceId { get; init; }
    public required string UserId { get; init; }
    public required string State { get; set; }
    public required DateTime Created { get; init; }
    public required DateTime Updated { get; set; }
    public DateTime? Completed { get; set; }
}
