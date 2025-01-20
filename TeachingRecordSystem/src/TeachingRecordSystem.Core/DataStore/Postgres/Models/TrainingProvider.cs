namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrainingProvider
{
    public required Guid TrainingProviderId { get; init; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
}
