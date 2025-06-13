namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class IntegrationTransaction
{
    public required long IntegrationTransactionId { get; set; }
    public required IntegrationTransactionInterfaceType InterfaceType { get; set; }
    public required IntegrationTransactionImportStatus ImportStatus { get; set; }
    public required int TotalCount { get; set; }
    public required int SuccessCount { get; set; }
    public required int FailureCount { get; set; }
    public required int DuplicateCount { get; set; }
    public required string FileName { get; set; }
    public required DateTime CreatedDate { get; set; }
    public ICollection<IntegrationTransactionRecord>? IntegrationTransactionRecords { get; }
}
