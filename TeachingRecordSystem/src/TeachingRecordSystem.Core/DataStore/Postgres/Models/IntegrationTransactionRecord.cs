namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class IntegrationTransactionRecord
{
    public required long IntegrationTransactionRecordId { get; set; }
    public required string? RowData { get; set; }
    public required string? FailureMessage { get; set; }
    public Person? Person { get; set; }
    public required Guid? PersonId { get; set; }
    public required bool? Duplicate { get; set; }
    public required IntegrationTransactionRecordStatus Status { get; set; }
    public required DateTime CreatedDate { get; set; }
    public long? IntegrationTransactionId { get; set; }
    public IntegrationTransaction? IntegrationTransaction { get; }
    public required bool? HasActiveAlert { get; set; }
}
