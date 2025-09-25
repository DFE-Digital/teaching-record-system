namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionStatusInfo
{
    public required InductionStatus InductionStatus { get; init; }
    public required string Name { get; set; }
}
