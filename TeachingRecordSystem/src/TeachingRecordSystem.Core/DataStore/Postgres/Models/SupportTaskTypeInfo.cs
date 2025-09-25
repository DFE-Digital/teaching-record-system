namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTaskTypeInfo
{
    public required SupportTaskType SupportTaskType { get; init; }
    public required string Name { get; set; }
}
