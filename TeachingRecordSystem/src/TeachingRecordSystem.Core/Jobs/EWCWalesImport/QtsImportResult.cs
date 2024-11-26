namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;

public record QtsImportResult(int TotalCount, int SuccessCount, int DuplicateCount, int FailureCount, string FailureMessage, Guid IntegrationTransactionId);
