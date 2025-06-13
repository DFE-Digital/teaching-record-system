namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class IntegrationTransactionRecord
{
    public required long IntegrationTransactionRecordId { get; set; }
    public required string? RowData { get; set; }
    public required string? FailureMessage { get; set; }
    public required Person Person { get; set; }
    public required Guid PersonId { get; set; }
    public required bool? Duplicate { get; set; }
    public required IntegrationTransactionRecordStatus Status { get; set; }
    public required DateTime CreatedDate { get; set; }
    public long? IntegrationTransactionId { get; }
    public IntegrationTransaction? IntegrationTransaction { get; }
}
