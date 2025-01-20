namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrainingSubject
{
    public required Guid TrainingSubjectId { get; init; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
}
