namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class JobMetadata
{
    public required string JobName { get; init; }
    public required Dictionary<string, object> Metadata { get; set; }
}
